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
using System.Data;
using Microsoft.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Dynamic;
using System.Net;
using System.Net.Mail;
using System.Text;
using Microsoft.AspNetCore.Mvc;
namespace QuickSoft.Controllers
{
	public class CrossHireReturnController : BaseController
	{
		ApplicationDbContext db;
		Common com;
		public CrossHireReturnController()
		{
			db = new ApplicationDbContext();
			com = new Common();
		}
		// GET: CrossHireReturn 
		[QkAuthorize(Roles = "Dev,CrossHireReturn List")]
		public ActionResult Index()
		{
			ViewBag.Supplier = QkSelect.List(
							   new List<SelectListItem>
							   {
									new SelectListItem { Selected = false, Text = "All", Value = "0"},
							   }, "Value", "Text", 1);

			ViewBag.SalesExecutive = QkSelect.List(
								new List<SelectListItem>
								{
									new SelectListItem { Selected = false, Text = "All", Value = "0"},
								}, "Value", "Text", 1);

			ViewBag.Invoice = QkSelect.List(
							   new List<SelectListItem>
							   {
									new SelectListItem { Selected = false, Text = "All", Value = "0"},
							   }, "Value", "Text", 1);

			var created = db.Users.Select(s => new
			{
				ID = s.Id,
				Name = s.UserName
			}).ToList();
			ViewBag.CreatedBy = QkSelect.List(created, "ID", "Name");

			var MlaHReturn = db.EnableSettings.Where(a => a.EnableType == "MLAHReturn").FirstOrDefault();
			var MlaHReturns = MlaHReturn != null ? MlaHReturn.Status : Status.inactive;
			ViewBag.MLAHReturn = MlaHReturns;

			var Stat = from ApprovalStatus e in Enum.GetValues(typeof(ApprovalStatus))
					   select new
					   {
						   ID = (int)e,
						   Name = e.ToString()
					   };
			ViewBag.AppStat = QkSelect.List(Stat, "Id", "Name");

			var Remind = db.EnableSettings.Where(a => a.EnableType == "RemindHReturn").FirstOrDefault();
			var Reminds = Remind != null ? Remind.Status : Status.inactive;
			ViewBag.Remind = Reminds;


            var EnableProject = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            var ProjectCheck = EnableProject == "ProjectBasedBusiness" ? Status.active : Status.inactive;
            ViewBag.ProjChk = ProjectCheck;

			var proj = db.Projects
			   .Select(s => new
			   {
				   ID = s.ProjectId,
				   Name = s.ProCode + " " + s.ProjectName
			   })
			   .ToList();
			ViewBag.getProj = QkSelect.List(proj, "ID", "Name");
			var tsk = db.ProTasks
			 .Select(s => new
			 {
				 ID = s.ProTaskId,
				 Name = s.TaskName
			 })
			 .ToList();
			ViewBag.getProTask = QkSelect.List(tsk, "ID", "Name");

			return View();
		}

		[HttpPost]
		[QkAuthorize(Roles = "Dev,CrossHireReturn List")]
		public ActionResult GetCrossHireReturn(string BillNo, string FromDate, string ToDate, long? supplier, long? salesperson, string user, long? invoice, string appstat, long? ProjectName, long? Task)
		{
			var draw = Request.Form.GetValues("draw").FirstOrDefault();
			int recordsTotal = 0;
			DateTime? fdate = null;
			DateTime? tdate = null;
			if (FromDate != "")
			{
				fdate = DateTime.Parse(FromDate, new CultureInfo("en-GB"));
			}
			if (ToDate != "")
			{
				tdate = DateTime.Parse(ToDate, new CultureInfo("en-GB"));
			}

			string search = Request.Form.GetValues("search[value]")[0];
			var start = Request.Form.GetValues("start").FirstOrDefault();
			var length = Request.Form.GetValues("length").FirstOrDefault();
			//Find Order Column
			var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
			var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();


			int pageSize = length != null ? Convert.ToInt32(length) : 0;
			int skip = start != null ? Convert.ToInt32(start) : 0;

			var fromv = "DVNote";
			var Tosales = "Sale";

			ApprovalStatus AppSt = new ApprovalStatus();
			if (appstat != "")
			{
				if (appstat == "0")
				{
					AppSt = ApprovalStatus.Approved;
				}
				else if (appstat == "1")
				{
					AppSt = ApprovalStatus.Rejected;
				}
				else
				{
					AppSt = ApprovalStatus.PendingApproval;
				}
			};

			var userpermission = User.IsInRole("All CrossHireReturn");
			var UserId = User.Identity.GetUserId();

			Employee empl = new Employee();
			empl.EmployeeId = db.Employees.Where(a => a.UserId == UserId).Select(a => a.EmployeeId).FirstOrDefault();

			var uDev = User.IsInRole("Dev");
			var uCrossHireReturnView = User.IsInRole("View CrossHireReturn");
			var uEdit = User.IsInRole("Edit CrossHireReturn");
			var uDownload = User.IsInRole("Download CrossHireReturn");
			var uDelete = User.IsInRole("Delete CrossHireReturn");

			// dc.ChangeTracker.LazyLoadingEnabled = false; // if your table is relational, contain foreign key
			var v = (from b in db.CrossHireReturns
					 join a in db.Suppliers on b.Supplier equals a.SupplierID
					 join d in db.Employees on b.Cashier equals d.EmployeeId into emp
					 from d in emp.DefaultIfEmpty()
					 join e in db.PurchaseEntrys on b.Invoice equals e.PurchaseEntryId into pur
					 from e in pur.DefaultIfEmpty()
					 join g in db.Users on b.CreatedUserId equals g.Id
					 join j in db.Projects on b.Project equals j.ProjectId into prj
					 from j in prj.DefaultIfEmpty()
					 join k in db.ProTasks on b.ProTask equals k.ProTaskId into task
					 from k in task.DefaultIfEmpty()

					 let qs = db.ConvertTransactionss.Where(ap => ap.From == b.HireReturnId && ap.ConvertFrom == fromv && ap.ConvertTo == Tosales).FirstOrDefault()
					 let mc = db.MCs.Where(x => x.AssignedUser == b.CreatedUserId).Select(x => x.MCId).FirstOrDefault()

					 let app = db.Approvals.Where(x => x.TransEntry == b.HireReturnId && x.Type == "CrossHireReturn").Select(x => x.EmployeeId).ToList()
					 let AppStatus = db.ApprovalUpdates.Where(x => x.TransEntry == b.HireReturnId && x.Type == "CrossHireReturn").Select(x => x.ApprovalStatus).ToList()
					 let chkAppStatus = db.ApprovalUpdates.Where(x => x.TransEntry == b.HireReturnId && x.Type == "CrossHireReturn").GroupBy(l => l.ApprovedBy)
										.Select(g => g.OrderByDescending(c => c.CreatedDate).FirstOrDefault().ApprovalStatus)
										.ToList()

					 where (BillNo == null || BillNo == "" || b.BillNo == BillNo) &&
					 (supplier == null || supplier == 0 || a.SupplierID == supplier) &&
					 (salesperson == null || salesperson == 0 || d.EmployeeId == salesperson) &&
					 (FromDate == null || FromDate == "" || EF.Functions.DateDiffDay(b.Date, fdate) <= 0) &&
					 (ToDate == null || ToDate == "" || EF.Functions.DateDiffDay(b.Date, tdate) >= 0) &&
					 (user == null || user == "" || g.Id == user) &&
					 (mc == 0 || mc == b.MaterialCenter) &&
					 (invoice == null || invoice == b.Invoice) &&
					 (userpermission == true || b.CreatedUserId == UserId)
					  && (ProjectName == 0 || ProjectName == null || j.ProjectId == ProjectName)
					 && (Task == 0 || Task == null || k.ProTaskId == Task)
					 select new
					 {
						 SaleConvert = (qs != null) ? qs.ConvertTo : "",
						 b.HireReturnId,
						 b.HrNo,
						 b.BillNo,
						 b.Date,
						 b.Items,
						 b.ItemQuantity,
						 EmpName = d.FirstName + " " + d.LastName,
						 supplier = a.SupplierCode + " - " + a.SupplierName,
						 User = g.UserName,
						 test = b.HireReturnId.ToString(),
						 a.Remark,
						 Invoice = e.BillNo,
						 Dev = uDev,
						 Details = uCrossHireReturnView,
						 Edit = uEdit,
						 Download = uDownload,
						 Delete = uDelete,

						 app = app,
						 AppStatus = AppStatus,
						 chkAppStatus = chkAppStatus,
						 ProjectName = (j.ProjectName != null && j.ProjectName != "") ? j.ProCode + "-" + j.ProjectName : "",
						 Task = (k.TaskName != null && k.TaskName != "") ? k.TaskCode + "-" + k.TaskName : "",
						 b.CreatedDate
					 }).ToList().Select(o => new
					 {

						 o.SaleConvert,
						 o.HireReturnId,
						 o.HrNo,
						 o.BillNo,
						 o.Date,
						 o.Items,
						 o.ItemQuantity,
						 o.EmpName,
						 o.supplier,
						 o.User,
						 o.test,
						 o.Remark,
						 o.Invoice,
						 o.Dev,
						 o.Details,
						 o.Edit,
						 o.Download,
						 o.Delete,
						 o.app,
						 Approval = (o.app != null && empl.EmployeeId != null) ? (o.app.Contains(empl.EmployeeId) ? true : false) : false,
						 ApprovalStatus = (o.app.Count > 0 && o.AppStatus.Count > 0 && o.chkAppStatus.Count > 0) ? (o.chkAppStatus.Contains(ApprovalStatus.Rejected) ? ApprovalStatus.Rejected : (o.app != null && o.chkAppStatus != null && o.app.Count != 0 && o.chkAppStatus.Count != 0 && o.chkAppStatus.Count() == o.app.Count() ? ApprovalStatus.Approved : ApprovalStatus.PendingApproval)) : ApprovalStatus.PendingApproval,
						 o.ProjectName,
						 o.Task,
						 o.CreatedDate
					 });
			if (appstat != "")
			{
				v = v.Where(a => a.ApprovalStatus == AppSt);
			}
			//search
			if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
			{
				// Apply search   
				v = v.Where(p => p.BillNo.ToString().ToLower().Equals(search.ToLower()));
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

		[QkAuthorize(Roles = "Dev,CrossHireReturn Entry")]
		public ActionResult Create(long? id, string type)
		{

			var userpermission = User.IsInRole("All CrossHireReturn");
			var UserId = User.Identity.GetUserId();

			var entry = new CrossHireReturnViewModel
			{
				BillNo = InvoiceNo(),
				Date = Convert.ToDateTime(System.DateTime.Now.ToShortDateString()),
				TermsCondition = db.TermsAndConditionss.Where(a => a.ConditionTypeID == "dvnote").Select(a => a.TermsCondit).FirstOrDefault()

			};

			var cust = db.Suppliers
				.Select(s => new
				{
					SupplierID = s.SupplierID,
					SupplierDetails = s.SupplierCode + " - " + s.SupplierName
				}).ToList();
			ViewBag.Custer = QkSelect.List(cust, "SupplierID", "SupplierDetails");
			var use = db.Employees
							 .Select(s => new
							 {
								 ID = s.EmployeeId,
								 Name = s.FirstName + " " + s.MiddleName + " " + s.LastName
							 })
							 .ToList();
			ViewBag.Cashier = QkSelect.List(use, "ID", "Name");
			companySet();


			var MCList1 = db.MCs.Where(a => a.AssignedUser == UserId).Select(a => (long?)a.MCId).ToList();
			var MCList = MCList1;
			if (!MCList.Any())
			{
				MCList = db.MCs.Select(a => (long?)a.MCId).ToList();
			}
			var MCArray = MCList.ToArray();


            ViewBag.LastEntry = db.CrossHireReturns.Where(p => MCArray.Contains(p.MaterialCenter) && (userpermission == true || p.CreatedUserId == UserId)).Select(p => p.HireReturnId).AsEnumerable().DefaultIfEmpty(0).Max();

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
				ViewBag.LastMc = mcs.Select(a => a.Id).FirstOrDefault();
			}
			else
			{
				var mcs = db.MCs.Select(s => new
				{
					Id = s.MCId,
					Name = s.MCName
				}).ToList();
				ViewBag.MC = QkSelect.List(mcs, "Id", "Name");
				ViewBag.LastMc = mcs.Select(a => a.Id).FirstOrDefault();
			}

			var mail = db.EnableSettings.Where(a => a.EnableType == "SaveAndMail").FirstOrDefault();
			var mailcheck = mail != null ? mail.Status : Status.inactive;
			ViewBag.CheckMail = mailcheck;

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

			var RemoveItemData = db.EnableSettings.Where(a => a.EnableType == "RemoveItemData").FirstOrDefault();
			var CheckItemData = RemoveItemData != null ? RemoveItemData.Status : Status.inactive;
			ViewBag.ItemDataCheck = CheckItemData;

			Int64? TimeOut = Convert.ToInt32(db.EnableSettings.Where(a => a.EnableType == "SetTimeOut").Select(a => a.TypeValue).FirstOrDefault());
			TimeOut = (TimeOut != null && TimeOut != 0) ? TimeOut : 10;
			ViewBag.TMOut = TimeOut;
			ViewBag.BusinessType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
			var hiretype = db.HireTypes
				 .Select(s => new
				 {
					 ID = s.HireTypeId,
					 Name = s.Name
				 })
				 .ToList();
			ViewBag.HiType = QkSelect.List(hiretype, "ID", "Name");

			ViewBag.DefMc = db.MCs.Where(a => a.MCName == "CrossHire").Select(a => a.MCId).FirstOrDefault();

            //         equals new { b1 = b.ConvertFrom, b2 = b.From } into conv
            //         into emp from d in emp.DefaultIfEmpty()
            //         where (a.SaleType == PurchaseHireType.CrossHire) && (a.PurchaseEntryId != b.From) && (a.PurchaseEntryId != d.HireReturnId)
            //             ID = a.PurchaseEntryId,
            //             Name = a.BillNo
            //         })

            var EnableProject = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            var ProjectCheck = EnableProject == "ProjectBasedBusiness" ? Status.active : Status.inactive;
            ViewBag.ProjChk = ProjectCheck;

			var proj = db.Projects
				.Select(s => new
				{
					ID = s.ProjectId,
					Name = s.ProCode + " " + s.ProjectName
				})
				.ToList();
			ViewBag.getProj = QkSelect.List(proj, "ID", "Name");
			var tsk = db.ProTasks
				.Select(s => new
				{
					ID = s.ProTaskId,
					Name = s.TaskName
				})
				.ToList();
			ViewBag.getProTask = QkSelect.List(tsk, "ID", "Name");

			ViewBag.PopUpAddCust = false;

			var appby = db.Employees.Where(a => a.UserStatus == true && a.UserId != UserId)
				  .Select(s => new
				  {
					  ID = s.EmployeeId,
					  Name = s.FirstName + " " + s.LastName
				  })
				  .ToList();
			ViewBag.ApproveBy = QkSelect.List(appby, "ID", "Name");

			var MlaHReturn = db.EnableSettings.Where(a => a.EnableType == "MLAHReturn").FirstOrDefault();
			var MlaHReturns = MlaHReturn != null ? MlaHReturn.Status : Status.inactive;
			ViewBag.MLAHReturn = MlaHReturns;

			var EnableHead = db.EnableSettings.Where(a => a.EnableType == "HideComHeaders").FirstOrDefault();
			var ComHeadCheck = EnableHead != null ? EnableHead.Status : Status.inactive;
			ViewBag.HeadCheck = ComHeadCheck;

			//field mapping
			entry.FieldMap = db.FieldMappings.Where(a => a.Section == "HReturn" && a.Status == Status.active).ToList();

			return View(entry);
		}

		[QkAuthorize(Roles = "Dev,CrossHireReturn Entry")]
		public JsonResult CreateCrossHireReturn(string[][] array, string[] dvdata, string action)
		{
			bool stat = false;
			string msg;
			if (ModelState.IsValid)
			{
				if (!BillExist(Convert.ToString(dvdata[7])))
				{
					Int64 dvId = 0;
					var EnablePartNumber = db.EnableSettings.Where(a => a.EnableType == "PartNoInItem").FirstOrDefault();
					var PartNoCheck = EnablePartNumber != null ? EnablePartNumber.Status : Status.inactive;

					var ItemCodeInPrint = db.EnableSettings.Where(a => a.EnableType == "EnableItemCodeInPrint").FirstOrDefault();
					var InPrintItemCode = ItemCodeInPrint != null ? ItemCodeInPrint.Status : Status.inactive;
					Int64 HrNo = 0;
					using (Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction dbTran = db.Database.BeginTransaction())
					{
						try
						{
							var UserId = User.Identity.GetUserId();
							long Branch = 0;
							var EnableBranch = db.EnableSettings.Where(a => a.EnableType == "EnableBranch").FirstOrDefault();
							var BranchCheck = EnableBranch != null ? EnableBranch.Status : Status.inactive;

							if (BranchCheck == Status.active)
							{
								Branch = Convert.ToInt64(dvdata[10]);
							}
							else
							{
								Branch = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).FirstOrDefault();
							}
							var EnableMC = db.EnableSettings.Where(a => a.EnableType == "MCInTransaction").FirstOrDefault();
							var MCcheck = EnableMC != null ? EnableMC.Status : Status.inactive;

							long MC = 0;
							if (MCcheck == Status.active)
							{
								MC = Convert.ToInt32(dvdata[9]);
							}
							else
							{
								MC = 1;
							}
							var Today = System.DateTime.Now;
							var EmailTemp = db.EmailTemplates.Where(a => a.Head == "CrossHireReturn").Select(a => a.EmailTemplateID).FirstOrDefault();
                            
							//sales entry
							HrNo = GetNo();
							CrossHireReturn entry = new CrossHireReturn();
							entry.HrNo = HrNo;
							entry.BillNo = Convert.ToString(dvdata[7]);
							entry.Supplier = Convert.ToInt64(dvdata[0]);
							entry.Cashier = dvdata[1] != "" ? Convert.ToInt64(dvdata[1]) : 0;
							entry.Date = DateTime.Parse(dvdata[2], new CultureInfo("en-GB"));
							entry.Items = Convert.ToInt32(dvdata[3]);
							entry.ItemQuantity = Convert.ToDecimal(dvdata[4]);
							entry.TermsCondition = Convert.ToString(dvdata[5]);
							entry.Remarks = dvdata[8];
							entry.RtType = "Return";
							entry.MaterialCenter = MC;
							entry.Note = "";
							entry.Mail = 0;
							entry.CreatedDate = Today;
							entry.CreatedUserId = UserId;
							entry.Status = Status.active;
							entry.EmailTemplateID = EmailTemp;
							entry.CompanyHeaderID = 0;
							entry.Branch = Branch;
							entry.Invoice = Convert.ToInt32(dvdata[11]);
							entry.Project = (dvdata[12] == "" || dvdata[12] == null) ? 0 : Convert.ToInt64(dvdata[12]);
							entry.ProTask = (dvdata[13] == "" || dvdata[13] == null ) ? 0 : Convert.ToInt64(dvdata[13]);

							entry.Ref1 = Convert.ToString(dvdata[15]);
							entry.Ref2 = Convert.ToString(dvdata[16]);
							entry.Ref3 = Convert.ToString(dvdata[17]);
							entry.Ref4 = Convert.ToString(dvdata[18]);
							entry.Ref5 = Convert.ToString(dvdata[19]);

							db.CrossHireReturns.Add(entry);
							db.SaveChanges();
							dvId = entry.HireReturnId;



							////// add to SEItem
							string result = string.Empty;
							DataTable dtItem = new DataTable();
							dtItem.Columns.Add("ItemUnit");
							dtItem.Columns.Add("ItemUnitPrice");
							dtItem.Columns.Add("ItemQuantity");
							dtItem.Columns.Add("ItemDiscount");
							dtItem.Columns.Add("ItemNote");

							dtItem.Columns.Add("ReceivedQty");
							dtItem.Columns.Add("DamageQty");
							dtItem.Columns.Add("MissingQty");


							dtItem.Columns.Add("Hr");
							dtItem.Columns.Add("Item");


							foreach (var arr in array)
							{
								DataRow dr = dtItem.NewRow();

								dr["ItemUnit"] = (arr[1] == null || arr[1] == "") ? null : (long?)Convert.ToInt64(arr[1]);
								dr["ItemUnitPrice"] = Convert.ToDecimal(arr[6]);
								dr["ItemQuantity"] = Convert.ToDecimal(arr[2]);
								dr["ItemDiscount"] = Convert.ToDecimal(arr[11]);
								dr["itemNote"] = Convert.ToString(arr[9].Replace("\n", "<br />"));

								dr["ReceivedQty"] = arr[3] != "" ? Convert.ToDecimal(arr[3]) : 0;
								dr["DamageQty"] = arr[4] != "" ? Convert.ToDecimal(arr[4]) : 0;
								dr["MissingQty"] = arr[5] != "" ? Convert.ToDecimal(arr[5]) : 0;

								dr["Hr"] = dvId;
								dr["Item"] = Convert.ToInt32(arr[0]);

								dtItem.Rows.Add(dr);
							}

							////// create parameter 
							SqlParameter parameter = new SqlParameter("@TableType", dtItem);
							parameter.SqlDbType = SqlDbType.Structured;
							parameter.TypeName = "TableTypeCrosshritems";
							//// execute sp sql 
							string sql = String.Format("EXEC {0} {1};", "SP_InsertCrosshritems", "@TableType");
							//// execute sql 
							db.Database.ExecuteSqlRaw(sql, parameter);
							//Approved By
							var Appby = Convert.ToString(dvdata[17]);
							if (Appby != null && Appby != "")
							{
								long[] Approve = Appby.Split(',').Select(Int64.Parse).ToArray();

								Approval approval = new Approval();
								foreach (var emp in Approve)
								{
									approval.TransEntry = dvId;
									approval.Type = "CrossHireReturn";
									approval.EmployeeId = emp;
									db.Approvals.Add(approval);
									db.SaveChanges();
								}
							}

							com.addlog(LogTypes.Created, UserId, "CrossHireReturn", "CrossHireReturns", findip(), dvId, "Successfully Submitted CrossHireReturn");


							//send mail to company address
							var sendmail = db.EnableSettings.Where(a => a.EnableType == "AutomaticMailInTransactions").FirstOrDefault();
							var sendcmail = sendmail != null ? sendmail.Status : Status.inactive;
							if (sendcmail == Status.active)
							{
								var custname = db.Suppliers.Where(a => a.SupplierID == entry.Supplier).Select(a => a.SupplierName).FirstOrDefault();
								var salesman = db.Employees.Where(a => a.EmployeeId == entry.Cashier).Select(a => a.FirstName + " " + a.LastName).FirstOrDefault();
								var username = db.Users.Where(a => a.Id == entry.CreatedUserId).Select(a => a.UserName).FirstOrDefault();

								var totrec = db.CrossHrItems.Where(a => a.Hr == entry.HireReturnId).Select(a => a.ReceivedQty).Sum() ?? 0;
								var totdam = db.CrossHrItems.Where(a => a.Hr == entry.HireReturnId).Select(a => a.DamageQty).Sum() ?? 0;
								var totmis = db.CrossHrItems.Where(a => a.Hr == entry.HireReturnId).Select(a => a.MissingQty).Sum() ?? 0;

								CompanyEmailFormat CEmail = new CompanyEmailFormat();
								CEmail.BillNo = "Hire Return-" + entry.BillNo;
								CEmail.Subject = "<table style='border: 1px solid black;border-collapse: collapse;padding: 5px;'><tr><td style='border: 1px solid black;border-collapse: collapse;padding: 5px;' colspan='2' align='center'><b>Hire Return Created</b></td><tr/> " +
										"<tr><td style='border: 1px solid black;border-collapse: collapse;padding: 5px;'>Date               :</td><td style='border: 1px solid black;border-collapse: collapse;padding: 5px;'> " + entry.Date.ToString("dd-MM-yyyy") + "</td><tr/>" +
										"<tr><td style='border: 1px solid black;border-collapse: collapse;padding: 5px;'>Supplier           :</td><td style='border: 1px solid black;border-collapse: collapse;padding: 5px;'> " + custname + "</td><tr/>" +
										"<tr><td style='border: 1px solid black;border-collapse: collapse;padding: 5px;'>Sales Executive    :</td><td style='border: 1px solid black;border-collapse: collapse;padding: 5px;'> " + salesman + "</td><tr/>" +
										"<tr><td style='border: 1px solid black;border-collapse: collapse;padding: 5px;'>Created Date       :</td><td style='border: 1px solid black;border-collapse: collapse;padding: 5px;'> " + entry.CreatedDate.ToString("dd-MM-yyyy hh:mm:ss tt") + "</td><tr/>" +
										"<tr><td style='border: 1px solid black;border-collapse: collapse;padding: 5px;'>Created User       :</td><td style='border: 1px solid black;border-collapse: collapse;padding: 5px;'> " + username + "</td><tr/>" +
										"<tr><td style='border: 1px solid black;border-collapse: collapse;padding: 5px;'>Against Invoice    :</td><td style='border: 1px solid black;border-collapse: collapse;padding: 5px;'> " + entry.Invoice + "</td><tr/>" +
										"<tr><td style='border: 1px solid black;border-collapse: collapse;padding: 5px;'>Total Received     :</td><td style='border: 1px solid black;border-collapse: collapse;padding: 5px;'> " + totrec + "</td><tr/>" +
										"<tr><td style='border: 1px solid black;border-collapse: collapse;padding: 5px;'>Total Damage       :</td><td style='border: 1px solid black;border-collapse: collapse;padding: 5px;'> " + totdam + "</td><tr/>" +
										"<tr><td style='border: 1px solid black;border-collapse: collapse;padding: 5px;'>Total Missing      :</td><td style='border: 1px solid black;border-collapse: collapse;padding: 5px;'> " + totmis + "</td><tr/></table>";

								com.SendToCompanyMail(CEmail);
							}




							dbTran.Commit();
						}
						catch (Exception ex)
						{
							dbTran.Rollback();
							msg = "Failed to Submit Return Note. " + ex.Message;
							stat = false;
							return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
						}
					}

					if (action == "print")
					{
						var fmapp = db.FieldMappings.Where(a => a.Section == "HReturn" && a.Print == FMPrint.Yes && a.Status == Status.active).ToList();

                        var EnableProject = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
                        var ProjectCheck = EnableProject == "ProjectBasedBusiness" ? Status.active : Status.inactive;

                        var EnableHead = db.EnableSettings.Where(a => a.EnableType == "HideComHeaders").FirstOrDefault();
						var ComHeadCheck = EnableHead != null ? EnableHead.Status : Status.inactive;


						Int64? TimeOut = Convert.ToInt32(db.EnableSettings.Where(a => a.EnableType == "SetTimeOut").Select(a => a.TypeValue).FirstOrDefault());
						TimeOut = (TimeOut != null && TimeOut != 0) ? TimeOut : 10;
						var HData = com.CrossHireReturnData(dvId, InPrintItemCode, PartNoCheck, TimeOut, ProjectCheck, ComHeadCheck);
						var item = HData.pdfItem.ToList();
						var summary = HData;

						var def = Convert.ToInt64(db.EnableSettings.Where(x => x.EnableType == "Invoice").Select(y => y.TypeValue).FirstOrDefault());
						def = def == 0 ? 1 : def;
						var layout = db.InvoiceLayouts.Where(a => a.Id == def).FirstOrDefault();

						stat = true;
						return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, item, summary, layout, fmapp } };
					}

					//        message.Body = "<p>DEAR SIR</p><p> Thank you for Contacting.</p>" +
					//            " <p>we are enclosing our delivery note for the items / services as requested by you during our discussions.<br/></p> " +

					//    return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
					else
					{
						msg = "Successfully submitted Return Note.";
						stat = true;
						return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
					}
				}
				else
				{
					msg = "Return Note No. Already Exists.";
					stat = false;
					return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };

				}

			}
			else
			{
				stat = false;
				msg = "Looks like something went wrong. Please check your form.";
				return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };

			}
		}


		[QkAuthorize(Roles = "Dev,Edit CrossHireReturn")]
		public ActionResult Edit(long? id)
		{
			if (id == null)
			{
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
			}
			var userpermission = User.IsInRole("All CrossHireReturn");
			var UserId = User.Identity.GetUserId();
			CrossHireReturn dvnote = db.CrossHireReturns.Where(x => (x.CreatedUserId == UserId || userpermission == true) && x.HireReturnId == id).FirstOrDefault();

			if (dvnote == null)
			{
				return NotFound();
			}
			CrossHireReturnViewModel vmodel = new CrossHireReturnViewModel();
			var cust = db.Suppliers
				.Select(s => new
				{
					SupplierID = s.SupplierID,
					SupplierDetails = s.SupplierCode + " - " + s.SupplierName
				}).ToList();
			ViewBag.Custer = QkSelect.List(cust, "SupplierID", "SupplierDetails");

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
				ViewBag.LastMc = mcs.Select(a => a.Id).FirstOrDefault();
			}
			else
			{
				var mcs = db.MCs.Select(s => new
				{
					Id = s.MCId,
					Name = s.MCName
				}).ToList();
				ViewBag.MC = QkSelect.List(mcs, "Id", "Name");
				ViewBag.LastMc = mcs.Select(a => a.Id).FirstOrDefault();
			}

			var use = db.Employees
							 .Select(s => new
							 {
								 ID = s.EmployeeId,
								 Name = s.FirstName + " " + s.MiddleName + " " + s.LastName
							 })
							 .ToList();
			ViewBag.Cashiers = QkSelect.List(use, "ID", "Name");

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
			
			vmodel = (from b in db.CrossHireReturns
					  join d in db.Suppliers on b.Supplier equals d.SupplierID into cst
					  from d in cst.DefaultIfEmpty()
					  join e in db.Contacts on d.Contact equals e.ContactID into cont
					  from e in cont.DefaultIfEmpty()
					  join f in db.PurchaseEntrys on b.Invoice equals f.PurchaseEntryId into sale
					  from f in sale.DefaultIfEmpty()
					  where b.HireReturnId == id
					  select new CrossHireReturnViewModel
					  {
						  HrNo = b.HrNo,
						  Supplier = b.Supplier,
						  Date = b.Date,
						  BillNo = b.BillNo,
						  Cashier = b.Cashier,
						  TermsCondition = b.TermsCondition,
						  custEmailId = e.EmailId,
						  Remarks = b.Remarks,
						  MaterialCenter = b.MaterialCenter,
						  Branch = b.Branch,
						  RtType = b.RtType,
						  Invoice = b.Invoice,
						  InvoiceNo = f.BillNo,
						  Project = b.Project,
						  ProTask = b.ProTask,
						  //convertBill = CBill,
						  //convertFrom = CType,
						  Ref1 = b.Ref1,
						  Ref2 = b.Ref2,
						  Ref3 = b.Ref3,
						  Ref4 = b.Ref4,
						  Ref5 = b.Ref5,
					  }).FirstOrDefault();
			companySet();

			var MCList1 = db.MCs.Where(a => a.AssignedUser == UserId).Select(a => (long?)a.MCId).ToList();
			var MCList = MCList1;
			if (!MCList.Any())
			{
				MCList = db.MCs.Select(a => (long?)a.MCId).ToList();
			}
			var MCArray = MCList.ToArray();

			ViewBag.preEntry = db.CrossHireReturns.Where(a => a.HireReturnId < id && MCArray.Contains(a.MaterialCenter) && (userpermission == true || a.CreatedUserId == UserId)).Select(a => a.HireReturnId).DefaultIfEmpty().Max();
			ViewBag.nxtEntry = db.CrossHireReturns.Where(a => a.HireReturnId > id && MCArray.Contains(a.MaterialCenter) && (userpermission == true || a.CreatedUserId == UserId)).Select(a => a.HireReturnId).DefaultIfEmpty().Min();

			var mail = db.EnableSettings.Where(a => a.EnableType == "SaveAndMail").FirstOrDefault();
			var mailcheck = mail != null ? mail.Status : Status.inactive;
			ViewBag.CheckMail = mailcheck;

			var RemoveItemData = db.EnableSettings.Where(a => a.EnableType == "RemoveItemData").FirstOrDefault();
			var CheckItemData = RemoveItemData != null ? RemoveItemData.Status : Status.inactive;
			ViewBag.ItemDataCheck = CheckItemData;

			Int64? TimeOut = Convert.ToInt32(db.EnableSettings.Where(a => a.EnableType == "SetTimeOut").Select(a => a.TypeValue).FirstOrDefault());
			TimeOut = (TimeOut != null && TimeOut != 0) ? TimeOut : 10;
			ViewBag.TMOut = TimeOut;

			ViewBag.DefMc = db.MCs.Where(a => a.MCName == "Hire").Select(a => a.MCId).FirstOrDefault();

            var EnableProject = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            var ProjectCheck = EnableProject == "ProjectBasedBusiness" ? Status.active : Status.inactive;
            ViewBag.ProjChk = ProjectCheck;

			var proj = db.Projects
			  .Select(s => new
			  {
				  ID = s.ProjectId,
				  Name = s.ProCode + " " + s.ProjectName
			  })
			  .ToList();
			ViewBag.getProj = QkSelect.List(proj, "ID", "Name");
			var tsk = db.ProTasks
				.Select(s => new
				{
					ID = s.ProTaskId,
					Name = s.TaskName
				})
				.ToList();
			ViewBag.getProTask = QkSelect.List(tsk, "ID", "Name");

			ViewBag.PopUpAddCust = false;

			var emp = db.Approvals.Where(a => a.TransEntry == id && a.Type == "CrossHireReturn").Select(a => a.EmployeeId).ToList();
			long[] empIds = emp.ToArray();

			var appby = db.Employees.Where(a => a.UserStatus == true && a.UserId != UserId)
				  .Select(s => new
				  {
					  FieldName = s.EmployeeId,
					  FieldID = s.FirstName + " " + s.LastName
				  })
				  .ToList();
			ViewBag.ApproveBy = new MultiSelectList(appby, "FieldName", "FieldID", empIds);

			var MlaHReturn = db.EnableSettings.Where(a => a.EnableType == "MLAHReturn").FirstOrDefault();
			var MlaHReturns = MlaHReturn != null ? MlaHReturn.Status : Status.inactive;
			ViewBag.MLAHReturn = MlaHReturns;

			var EditPermission = User.IsInRole("Disable HReturn Edit After Approval");
			ViewBag.ChkApp = com.chkApproved((long)id, EditPermission, "CrossHireReturn", UserId);

			var EnableHead = db.EnableSettings.Where(a => a.EnableType == "HideComHeaders").FirstOrDefault();
			var ComHeadCheck = EnableHead != null ? EnableHead.Status : Status.inactive;
			ViewBag.HeadCheck = ComHeadCheck;

			//field mapping
			vmodel.FieldMap = db.FieldMappings.Where(a => a.Section == "HReturn" && a.Status == Status.active).ToList();

            //dummy table operations
            var DItem = db.DummyCrossHrItems.Where(a => a.Hr == id).FirstOrDefault();
            var CItem = db.CrossHrItems.Where(a => a.Hr == id).FirstOrDefault();
            if (CItem == null && DItem != null)
            {
                var DItems = db.DummyCrossHrItems.Where(a => a.Hr == id).ToList();
                foreach (var arr in DItems)
                {
                    //add to se-item table
                    CrossHrItem sItem = new CrossHrItem();
                    sItem.ItemUnit = arr.ItemUnit;
                    sItem.ItemUnitPrice = arr.ItemUnitPrice;
                    sItem.ItemQuantity = arr.ItemQuantity;
                    sItem.ItemDiscount = arr.ItemDiscount;
                    sItem.ReceivedQty = arr.ReceivedQty;
                    sItem.DamageQty = arr.DamageQty;
                    sItem.MissingQty = arr.MissingQty;
                    sItem.ItemNote = arr.ItemNote;
                    sItem.Hr = arr.Hr;
                    sItem.Item = arr.Item;

                    db.CrossHrItems.Add(sItem);
                    db.SaveChanges();
                }

                db.DummyCrossHrItems.RemoveRange(db.DummyCrossHrItems.Where(a => a.Hr == id));
                db.SaveChanges();
            }

            return View(vmodel);
		}

		[QkAuthorize(Roles = "Dev,Edit CrossHireReturn")]
		public JsonResult UpdateCrossHireReturn(string[][] array, string[] dvdata, string action)
		{
			bool stat = false;
			string msg;
			if (ModelState.IsValid)
			{
				Int64 dvId = Convert.ToInt64(dvdata[14]);
				CrossHireReturn entry = db.CrossHireReturns.Find(dvId);

				if (BillExist(Convert.ToString(dvdata[6])) && Convert.ToString(dvdata[7]) != entry.BillNo)
				{
					msg = "Invoice No. Already Exists.";
					stat = false;
					return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
				}

				var EnablePartNumber = db.EnableSettings.Where(a => a.EnableType == "PartNoInItem").FirstOrDefault();
				var PartNoCheck = EnablePartNumber != null ? EnablePartNumber.Status : Status.inactive;

				var ItemCodeInPrint = db.EnableSettings.Where(a => a.EnableType == "EnableItemCodeInPrint").FirstOrDefault();
				var InPrintItemCode = ItemCodeInPrint != null ? ItemCodeInPrint.Status : Status.inactive;
				Int64 HrNo = 0;
				using (Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction dbTran = db.Database.BeginTransaction())
				{
					try
					{
						var UserId = User.Identity.GetUserId();
						var UserName = db.Users.Where(a => a.Id == UserId).Select(a => a.Name).First();

						var BranchID = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).First();
						long Branch = 0;

						var EnableBranch = db.EnableSettings.Where(a => a.EnableType == "EnableBranch").FirstOrDefault();
						var BranchCheck = EnableBranch != null ? EnableBranch.Status : Status.inactive;

						if (BranchCheck == Status.active)
						{
							Branch = Convert.ToInt64(dvdata[10]);
						}
						else
						{
							Branch = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).FirstOrDefault();
						}

						var EnableMC = db.EnableSettings.Where(a => a.EnableType == "MCInTransaction").FirstOrDefault();
						var MCcheck = EnableMC != null ? EnableMC.Status : Status.inactive;

						long MC = 0;
						if (MCcheck == Status.active)
						{
							MC = Convert.ToInt32(dvdata[9]);
						}
						else
						{
							MC = 1;
						}

						var EditPermission = User.IsInRole("Disable HReturn Edit After Approval");
						if (com.chkApproved(dvId, EditPermission, "CrossHireReturn", UserId) == true)
						{
							entry.HrNo = HrNo;
							entry.BillNo = Convert.ToString(dvdata[7]);
							entry.Supplier = Convert.ToInt64(dvdata[0]);
							entry.Cashier = (dvdata[1] != null && dvdata[1] != "") ? Convert.ToInt64(dvdata[1]) : 0;
							entry.Date = DateTime.Parse(dvdata[2], new CultureInfo("en-GB"));
							entry.Items = Convert.ToInt32(dvdata[3]);
							entry.ItemQuantity = Convert.ToDecimal(dvdata[4]);
							entry.TermsCondition = Convert.ToString(dvdata[5]);
							entry.Remarks = dvdata[8];
							entry.RtType = "Return";
							entry.MaterialCenter = MC;
							entry.Note = "";
							entry.Mail = 0;
							entry.Status = Status.active;
							entry.CompanyHeaderID = 0;
							entry.Branch = Branch;
							entry.Project = dvdata[12] != "" ? Convert.ToInt64(dvdata[12]) : 0;
							entry.ProTask = dvdata[13] != "" ? Convert.ToInt64(dvdata[13]) : 0;
							entry.Invoice = Convert.ToInt32(dvdata[11]);


							entry.EmailTemplateID = db.EmailTemplates.Where(a => a.Head == "CrossHireReturn").Select(a => a.EmailTemplateID).FirstOrDefault();

							entry.Ref1 = Convert.ToString(dvdata[16]);
							entry.Ref2 = Convert.ToString(dvdata[17]);
							entry.Ref3 = Convert.ToString(dvdata[18]);
							entry.Ref4 = Convert.ToString(dvdata[19]);
							entry.Ref5 = Convert.ToString(dvdata[20]);

							db.Entry(entry).State = EntityState.Modified;
							db.SaveChanges();
							Int64 delyId = entry.HireReturnId;
							HrNo = entry.HrNo;
							var HireItem = db.HireDetails.Where(a => a.Reference == delyId && a.Section == "Delivernote").FirstOrDefault();
							if (HireItem != null)
							{
								db.HireDetails.RemoveRange(db.HireDetails.Where(a => a.Reference == delyId && a.Section == "Delivernote"));
								db.SaveChanges();
							}

							var DVItem = db.CrossHrItems.Where(a => a.Hr == delyId).FirstOrDefault();
							if (DVItem != null)
							{
                                var SItems = db.CrossHrItems.Where(a => a.Hr == delyId).ToList();
                                foreach (var arr in SItems)
                                {
                                    //add to dummy table
                                    DummyCrossHrItem dItem = new DummyCrossHrItem();
                                    dItem.ItemUnit = arr.ItemUnit;
                                    dItem.ItemUnitPrice = arr.ItemUnitPrice;
                                    dItem.ItemQuantity = arr.ItemQuantity;
                                    dItem.ItemDiscount = arr.ItemDiscount;
                                    dItem.ReceivedQty = arr.ReceivedQty;
                                    dItem.DamageQty = arr.DamageQty;
                                    dItem.MissingQty = arr.MissingQty;
                                    dItem.ItemNote = arr.ItemNote;
                                    dItem.Hr = arr.Hr;
                                    dItem.Item = arr.Item;
                                    db.DummyCrossHrItems.Add(dItem);
                                    db.SaveChanges();
                                }

                                db.CrossHrItems.RemoveRange(db.CrossHrItems.Where(a => a.Hr == delyId));
								db.SaveChanges();
							}

							////// add to SEItem
							string result = string.Empty;
							DataTable dtItem = new DataTable();
							dtItem.Columns.Add("ItemUnit");
							dtItem.Columns.Add("ItemUnitPrice");
							dtItem.Columns.Add("ItemQuantity");
							dtItem.Columns.Add("ItemDiscount");
							dtItem.Columns.Add("ItemNote");

							dtItem.Columns.Add("ReceivedQty");
							dtItem.Columns.Add("DamageQty");
							dtItem.Columns.Add("MissingQty");


							dtItem.Columns.Add("Hr");
							dtItem.Columns.Add("Item");



							foreach (var arr in array)
							{
								DataRow dr = dtItem.NewRow();

								dr["ItemUnit"] = (arr[1] == null || arr[1] == "") ? null : (long?)Convert.ToInt64(arr[1]);
								dr["ItemUnitPrice"] = Convert.ToDecimal(arr[6]);
								dr["ItemQuantity"] = Convert.ToDecimal(arr[2]);
								dr["ItemDiscount"] = Convert.ToDecimal(arr[11]);
								dr["itemNote"] = Convert.ToString(arr[9].Replace("\n", "<br />"));

								dr["ReceivedQty"] = arr[3] != "" ? Convert.ToDecimal(arr[3]) : 0;
								dr["DamageQty"] = arr[4] != "" ? Convert.ToDecimal(arr[4]) : 0;
								dr["MissingQty"] = arr[5] != "" ? Convert.ToDecimal(arr[5]) : 0;

								dr["Hr"] = dvId;
								dr["Item"] = Convert.ToInt32(arr[0]);

								dtItem.Rows.Add(dr);
							}

							////// create parameter 
							SqlParameter parameter = new SqlParameter("@TableType", dtItem);
							parameter.SqlDbType = SqlDbType.Structured;
							parameter.TypeName = "TableTypeCrosshritems";
							//// execute sp sql 
							string sql = String.Format("EXEC {0} {1};", "SP_InsertCrosshritems", "@TableType");
                            //// execute sql 
                            var ret = db.Database.ExecuteSqlRaw(sql, parameter);
                            if (ret > 0)
                            {
                                db.DummyCrossHrItems.RemoveRange(db.DummyCrossHrItems.Where(a => a.Hr == delyId));
                                db.SaveChanges();
                            }


                            //Approved By
                            var empuser = db.Employees.Where(a => a.UserId == UserId).Select(a => a.EmployeeId).FirstOrDefault();
							var chkapp = db.Approvals.Where(a => a.EmployeeId == empuser && a.TransEntry == dvId && a.Type == "CrossHireReturn").FirstOrDefault();

							var MrnPO = db.Approvals.Where(a => a.TransEntry == dvId && a.Type == "CrossHireReturn").FirstOrDefault();
							if (MrnPO != null)
							{
								if (chkapp != null)
								{
									db.Approvals.RemoveRange(db.Approvals.Where(a => a.EmployeeId != empuser && a.TransEntry == dvId && a.Type == "CrossHireReturn"));
									db.SaveChanges();
								}
								else
								{

									db.Approvals.RemoveRange(db.Approvals.Where(a => a.TransEntry == dvId && a.Type == "CrossHireReturn"));
									db.SaveChanges();
								}
							}
							var Appby = Convert.ToString(dvdata[18]);
							if (Appby != null && Appby != "")
							{
								long[] Approve = Appby.Split(',').Select(Int64.Parse).ToArray();

								Approval approval = new Approval();
								foreach (var emp in Approve)
								{
									approval.TransEntry = dvId;
									approval.Type = "CrossHireReturn";
									approval.EmployeeId = emp;
									db.Approvals.Add(approval);
									db.SaveChanges();
								}
							}
							com.addlog(LogTypes.Updated, UserId, "CrossHireReturn", "CrossHireReturns", findip(), dvId, "Successfully Updated CrossHireReturn");
						}
						//send mail to company address
						var sendmail = db.EnableSettings.Where(a => a.EnableType == "AutomaticMailInTransactions").FirstOrDefault();
						var sendcmail = sendmail != null ? sendmail.Status : Status.inactive;
						if (sendcmail == Status.active)
						{
							var custname = db.Suppliers.Where(a => a.SupplierID == entry.Supplier).Select(a => a.SupplierName).FirstOrDefault();
							var salesman = db.Employees.Where(a => a.EmployeeId == entry.Cashier).Select(a => a.FirstName + " " + a.LastName).FirstOrDefault();
							var username = db.Users.Where(a => a.Id == entry.CreatedUserId).Select(a => a.UserName).FirstOrDefault();

							var totrec = db.CrossHrItems.Where(a => a.Hr == entry.HireReturnId).Select(a => a.ReceivedQty).Sum() ?? 0;
							var totdam = db.CrossHrItems.Where(a => a.Hr == entry.HireReturnId).Select(a => a.DamageQty).Sum() ?? 0;
							var totmis = db.CrossHrItems.Where(a => a.Hr == entry.HireReturnId).Select(a => a.MissingQty).Sum() ?? 0;

							CompanyEmailFormat CEmail = new CompanyEmailFormat();
							CEmail.BillNo = "Hire Return-" + entry.BillNo;
							CEmail.Subject = "<table style='border: 1px solid black;border-collapse: collapse;padding: 5px;'><tr><td style='border: 1px solid black;border-collapse: collapse;padding: 5px;' colspan='2' align='center'><b>Hire Return Updated</b></td><tr/> " +
									"<tr><td style='border: 1px solid black;border-collapse: collapse;padding: 5px;'>Date               :</td><td style='border: 1px solid black;border-collapse: collapse;padding: 5px;'> " + entry.Date.ToString("dd-MM-yyyy") + "</td><tr/>" +
									"<tr><td style='border: 1px solid black;border-collapse: collapse;padding: 5px;'>Supplier           :</td><td style='border: 1px solid black;border-collapse: collapse;padding: 5px;'> " + custname + "</td><tr/>" +
									"<tr><td style='border: 1px solid black;border-collapse: collapse;padding: 5px;'>Sales Executive    :</td><td style='border: 1px solid black;border-collapse: collapse;padding: 5px;'> " + salesman + "</td><tr/>" +
									"<tr><td style='border: 1px solid black;border-collapse: collapse;padding: 5px;'>Created Date       :</td><td style='border: 1px solid black;border-collapse: collapse;padding: 5px;'> " + entry.CreatedDate.ToString("dd-MM-yyyy hh:mm:ss tt") + "</td><tr/>" +
									"<tr><td style='border: 1px solid black;border-collapse: collapse;padding: 5px;'>Created User       :</td><td style='border: 1px solid black;border-collapse: collapse;padding: 5px;'> " + username + "</td><tr/>" +
									"<tr><td style='border: 1px solid black;border-collapse: collapse;padding: 5px;'>Against Invoice    :</td><td style='border: 1px solid black;border-collapse: collapse;padding: 5px;'> " + entry.Invoice + "</td><tr/>" +
									"<tr><td style='border: 1px solid black;border-collapse: collapse;padding: 5px;'>Total Received     :</td><td style='border: 1px solid black;border-collapse: collapse;padding: 5px;'> " + totrec + "</td><tr/>" +
									"<tr><td style='border: 1px solid black;border-collapse: collapse;padding: 5px;'>Total Damage       :</td><td style='border: 1px solid black;border-collapse: collapse;padding: 5px;'> " + totdam + "</td><tr/>" +
									"<tr><td style='border: 1px solid black;border-collapse: collapse;padding: 5px;'>Total Missing      :</td><td style='border: 1px solid black;border-collapse: collapse;padding: 5px;'> " + totmis + "</td><tr/></table>";

							com.SendToCompanyMail(CEmail);
						}

						dbTran.Commit();

					}
					catch (Exception ex)
					{
						dbTran.Rollback();
						msg = "Failed to Update Return Note.";
						stat = false;
						return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
					}
				}

				if (action == "print")
				{
					var fmapp = db.FieldMappings.Where(a => a.Section == "HReturn" && a.Print == FMPrint.Yes && a.Status == Status.active).ToList();

					Int64? TimeOut = Convert.ToInt32(db.EnableSettings.Where(a => a.EnableType == "SetTimeOut").Select(a => a.TypeValue).FirstOrDefault());
					TimeOut = (TimeOut != null && TimeOut != 0) ? TimeOut : 10;

                    var EnableProject = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
                    var ProjectCheck = EnableProject == "ProjectBasedBusiness" ? Status.active : Status.inactive;


                    var EnableHead = db.EnableSettings.Where(a => a.EnableType == "HideComHeaders").FirstOrDefault();
					var ComHeadCheck = EnableHead != null ? EnableHead.Status : Status.inactive;

					var HData = com.CrossHireReturnData(dvId, InPrintItemCode, PartNoCheck, TimeOut, ProjectCheck, ComHeadCheck);
					var item = HData.pdfItem.ToList();
					var summary = HData;

					var def = Convert.ToInt64(db.EnableSettings.Where(x => x.EnableType == "Invoice").Select(y => y.TypeValue).FirstOrDefault());
					def = def == 0 ? 1 : def;
					var layout = db.InvoiceLayouts.Where(a => a.Id == def).FirstOrDefault();

					stat = true;
					return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, item, summary, layout, fmapp } };

				}

				//        message.Body = "<p>DEAR SIR</p><p> Thank you for Contacting.</p>" +
				//            " <p>we are enclosing our delivery note for the items / services as requested by you during our discussions.<br/></p> " +

				//    return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
				else
				{
					msg = "Successfully Updated Return Note.";
					stat = true;
					return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
				}
			}
			else
			{
				stat = false;
				msg = "Looks like something went wrong. Please check your form.";
				return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
			}

		}

		[QkAuthorize(Roles = "Dev,Delete CrossHireReturn")]
		public ActionResult Delete(long? id)
		{
			if (id == null)
			{
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
			}
			var userpermission = User.IsInRole("All CrossHireReturn");
			var UserId = User.Identity.GetUserId();
			CrossHireReturn Hr = db.CrossHireReturns.Where(x => (x.CreatedUserId == UserId || userpermission == true) && x.HireReturnId == id).FirstOrDefault();

			if (Hr == null)
			{
				return NotFound();
			}
			return PartialView();
		}

		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		[QkAuthorize(Roles = "Dev,Delete CrossHireReturn")]
		public ActionResult DeleteConfirmed(long id)
		{
			bool stat = false;
			string msg;

			var chk = DeleteHR(id);
			if (chk == true)
			{
				stat = true;
				msg = "Successfully deleted Return Note.";
			}
			else
			{
				stat = false;
				msg = "Looks like something went wrong. Please check your form.";
			}
			return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
		}

		[HttpPost]
		[QkAuthorize(Roles = "Dev,Delete CrossHireReturn")]
		public ActionResult DeleteAllNote(long[] bill)
		{
			Int32 count = 0;
			foreach (var arr in bill)
			{
				var chk = (DeleteHR(arr) == true) ? count++ : count;
			}
			Success("Deleted " + count + " Return Note.", true);
			return RedirectToAction("Index", "CrossHireReturn");
		}

		[HttpGet]
		//[QkAuthorize(Roles = "Dev,Download CrossHireReturn")]
		public ActionResult Download(long id)
		{
			var cname = db.companys.Select(s => s.CPName).FirstOrDefault();
			var billno = db.CrossHireReturns.Where(s => s.HireReturnId == id).Select(s => s.BillNo).FirstOrDefault();

			SendMail sm = new SendMail();
			byte[] ms = sm.DownloadPdf(generatePdf(id), "inactive");
			return File(ms, "application/pdf", cname + "_CrossHireReturn_" + billno + "_" + System.DateTime.Now.ToShortDateString() + ".pdf");
		}
		public StringBuilder generatePdf(long id)
		{
			var EnablePartNumber = db.EnableSettings.Where(a => a.EnableType == "PartNoInItem").FirstOrDefault();
			var PartNoCheck = EnablePartNumber != null ? EnablePartNumber.Status : Status.inactive;

			var ItemCodeInPrint = db.EnableSettings.Where(a => a.EnableType == "EnableItemCodeInPrint").FirstOrDefault();
			var InPrintItemCode = ItemCodeInPrint != null ? ItemCodeInPrint.Status : Status.inactive;

			Int64? TimeOut = Convert.ToInt32(db.EnableSettings.Where(a => a.EnableType == "SetTimeOut").Select(a => a.TypeValue).FirstOrDefault());
			TimeOut = (TimeOut != null && TimeOut != 0) ? TimeOut : 10;

			var HData = com.CrossHireReturnData(id, InPrintItemCode, PartNoCheck, TimeOut);
			var item = HData.pdfItem.ToList();
			var summary = HData;

			return com.generatepdf(id, summary, item, null, "Hire Return");
		}
		[HttpGet]
		[QkAuthorize(Roles = "Dev,View CrossHireReturn")]
		public ActionResult Details(long? id)
		{
			var EnableMC = db.EnableSettings.Where(a => a.EnableType == "MCInTransaction").FirstOrDefault();
			var MCcheck = EnableMC != null ? EnableMC.Status : Status.inactive;
			ViewBag.EnableMCcheck = MCcheck;

			var EnablePartNumber = db.EnableSettings.Where(a => a.EnableType == "PartNoInItem").FirstOrDefault();
			var PartNoCheck = EnablePartNumber != null ? EnablePartNumber.Status : Status.inactive;
			ViewBag.PartNoCheck = PartNoCheck;

			CrossHireReturnViewModel vmodel = new CrossHireReturnViewModel();
			vmodel = (from b in db.CrossHireReturns
					  join c in db.Suppliers on b.Supplier equals c.SupplierID into cust
					  from c in cust.DefaultIfEmpty()
					  join d in db.Contacts on c.Contact equals d.ContactID into cnt
					  from d in cnt.DefaultIfEmpty()
					  join e in db.Employees on b.Cashier equals e.EmployeeId into user
					  from e in user.DefaultIfEmpty()
					  join f in db.MCs on b.MaterialCenter equals f.MCId into mcs
					  from f in mcs.DefaultIfEmpty()

                      join p in db.PurchaseEntrys on b.Invoice equals p.PurchaseEntryId into pur
                      from p in pur.DefaultIfEmpty()
                      join u in db.HireDetails on new { h1 = p.PurchaseEntryId, h2 = "Purchase" }
                      equals new { h1 = u.Reference, h2 = u.Section } into hir
                      from u in hir.DefaultIfEmpty()
                      join x in db.HireTypes on u.HireType equals x.HireTypeId into htyp
                      from x in htyp.DefaultIfEmpty()
                      where b.HireReturnId == id
					  select new CrossHireReturnViewModel
					  {
						  SupplierName = c.SupplierCode + " - " + c.SupplierName,
						  HrNo = b.HrNo,
						  BillNo = b.BillNo,
						  Date = b.Date,
						  TermsCondition = b.TermsCondition.Replace("\n", "<br />"),
						  EmployeeName = e.FirstName + " " + e.LastName,
						  Remarks = b.Remarks.Replace("\n", "<br />"),
						  MCName = f.MCName,
						  RtType = b.RtType,
						  Ref1 = b.Ref1,
						  Ref2 = b.Ref2,
						  Ref3 = b.Ref3,
						  Ref4 = b.Ref4,
						  Ref5 = b.Ref5,

                          InBillNo=p.BillNo,
                          HType=x.Name,
                          Emp = (from ab in db.Approvals
                                 join bb in db.Employees on ab.EmployeeId equals bb.EmployeeId
                                 where ab.TransEntry == id && ab.Type == "CrossHireReturn"
                                 select new ApprovalViewModel
                                 {
                                     EmpName = bb.FirstName + " " + bb.LastName
                                 }).ToList()
                      }).FirstOrDefault();

			var v = (from b in db.CrossHrItems
					 join c in db.Items on b.Item equals c.ItemID
					 join d in db.ItemImages on b.Item equals d.ItemID into itimg
					 from d in itimg.DefaultIfEmpty()
					 join e in db.ItemUnits on b.ItemUnit equals e.ItemUnitID into itunit
					 from e in itunit.DefaultIfEmpty()
					 join g in db.ItemBundles on c.ItemID equals g.mainItem into bundle
					 from g in bundle.DefaultIfEmpty()
					 join h in db.CrossHireReturns on b.Hr equals h.HireReturnId
					 where b.Hr == id && b.ItemNote != "-:{Bundle_Item}"
					 select new
					 {
						 b.ItemUnitPrice,
						 b.ItemQuantity,
						 b.ItemNote,
						 c.ItemCode,
						 c.ItemName,
						 e.ItemUnitName,
						 c.PartNumber,

						 b.ReceivedQty,
						 b.DamageQty,
						 b.MissingQty,
						 b.Item,

						 RetQty = (decimal?)(from aa in db.CrossHrItems
											 join bb in db.CrossHireReturns on aa.Hr equals bb.HireReturnId
											 where aa.Item == b.Item && bb.Invoice == h.Invoice
											 && aa.ItemNote != "-:{Bundle_Item}"
											 select new
											 {
												 aa.ItemQuantity
											 }).Sum(a => a.ItemQuantity) ?? 0,

						 DvQty = (decimal?)(from ab in db.PEItemss
											join ba in db.PurchaseEntrys on ab.PurchaseEntry equals ba.PurchaseEntryId
											where ab.Item == b.Item && ba.PurType == PurchaseHireType.CrossHire
											&& ba.PurchaseEntryId == h.Invoice
											&& ab.itemNote != "-:{Bundle_Item}"
											select new
											{
												ab.ItemQuantity
											}).Sum(a => a.ItemQuantity) ?? 0,
					 }).ToList();

			vmodel.HrItem = (from o in v
							 select new HrItemViewModel
							 {
								 ItemUnitPrice = o.ItemUnitPrice,
								 ItemQuantity = o.ItemQuantity,
								 ItemNote = o.ItemNote,
								 ItemCode = o.ItemCode,
								 ItemName = o.ItemName,
								 ItemUnit = o.ItemUnitName,
								 PartNumber = o.PartNumber,

								 ReceivedQty = o.ReceivedQty,
								 DamageQty = o.DamageQty,
								 MissingQty = o.MissingQty,

								 bundleitem = (from ab in db.CrossHrItems
											   join bb in db.Items on ab.Item equals bb.ItemID
											   join cb in db.ItemUnits on bb.ItemUnitID equals cb.ItemUnitID into primary
											   from cb in primary.DefaultIfEmpty()
											   join bd in db.ItemUnits on bb.SubUnitId equals bd.ItemUnitID into second
											   from bd in second.DefaultIfEmpty()
											   join zz in db.CrossHireReturns on ab.Hr equals zz.HireReturnId
											   where ab.Hr == id && ab.ItemNote == "-:{Bundle_Item}"
											   && o.Item == ab.ItemDiscount
											   select new ItemDetailViewModel
											   {
												   ItemCode = bb.ItemCode,
												   ItemName = bb.ItemName,
												   ItemUnit = cb.ItemUnitName,
												   ItemQuantity = ab.ItemQuantity,

												   ReceivedQty = ab.ReceivedQty,
												   DamageQty = ab.DamageQty,
												   MissingQty = ab.MissingQty,


												   RetQty = (decimal?)(from xx in db.CrossHrItems
																	   join yy in db.CrossHireReturns on xx.Hr equals yy.HireReturnId
																	   where xx.ItemNote == "-:{Bundle_Item}"
																	   && yy.HireReturnId == id
																	   && ab.Item == xx.Item
																	   && xx.ItemDiscount == ab.ItemDiscount
																	   select new
																	   {
																		   xx.ItemQuantity
																	   }).Sum(a => a.ItemQuantity) ?? 0,

												   DvQty = (decimal?)(from xx in db.PEItemss
																	  join yy in db.PurchaseEntrys on xx.PurchaseEntry equals yy.PurchaseEntryId
																	  where xx.itemNote == "-:{Bundle_Item}"
																	  && yy.PurchaseEntryId == zz.Invoice
																	  && ab.Item == xx.Item
																	  && xx.ItemDiscount == ab.ItemDiscount
																	  select new
																	  {
																		  xx.ItemQuantity
																	  }).Sum(a => a.ItemQuantity) ?? 0,

											   }).ToList(),

							 }).ToList();
			//field mapping
			vmodel.FieldMap = db.FieldMappings.Where(a => a.Section == "HReturn" && a.Status == Status.active).ToList();

			return View(vmodel);
		}

		[HttpGet]
		public ActionResult GetSupplier(int SupplierID)
		{
			var email = (from b in db.CrossHireReturns
						 join c in db.Suppliers on b.Supplier equals c.SupplierID into cust
						 from c in cust.DefaultIfEmpty()
						 join d in db.Contacts on c.Contact equals d.ContactID into cnt
						 from d in cnt.DefaultIfEmpty()
						 where b.Supplier == SupplierID
						 select new
						 {
							 CustomerEmail = d.EmailId,
						 }).FirstOrDefault();
			return Json(email);

		}

		[HttpGet]
		public ActionResult GetCrosshritems(long entryId, long? ItemId)
		{

			var v = (from a in db.CrossHrItems
					 join b in db.Items on a.Item equals b.ItemID
					 join c in db.ItemUnits on b.ItemUnitID equals c.ItemUnitID into primary
					 from c in primary.DefaultIfEmpty()
					 join d in db.ItemUnits on b.SubUnitId equals d.ItemUnitID into second
					 from d in second.DefaultIfEmpty()
					 join e in db.ItemCategorys on b.ItemCategoryID equals e.ItemCategoryID into cat
					 from e in cat.DefaultIfEmpty()
					 join f in db.Taxs on b.TaxID equals f.TaxID into taxss
					 from f in taxss.DefaultIfEmpty()
					 join g in db.CrossHireReturns on a.Hr equals g.HireReturnId
					 where a.Hr == entryId && a.ItemNote != "-:{Bundle_Item}"
					 && (ItemId == null || a.Item == ItemId) //&& b.KeepStock == true
					 select new
					 {
						 a.Item,
						 a.ItemQuantity,
						 a.ItemUnit,
						 a.ItemUnitPrice,
						 a.ItemDiscount,
						 note = a.ItemNote.Replace("<br />", "\n"),

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
						 categoryname = e.ItemCategoryName,
						 Tax = f.Percentage,
						 b.SellingPrice,
						 b.PurchasePrice,
						 b.BasePrice,
						 b.MRP,
						 b.KeepStock,

						 a.ReceivedQty,
						 a.DamageQty,
						 a.MissingQty,


						 RetItemQuantity = (decimal?)(from aa in db.CrossHrItems
													  join bb in db.CrossHireReturns on aa.Hr equals bb.HireReturnId
													  where aa.Item == a.Item && bb.Invoice == g.Invoice
													  && aa.ItemNote != "-:{Bundle_Item}"
													  select new
													  {
														  aa.ItemQuantity
													  }).Sum(a => a.ItemQuantity) ?? 0,

						 DvItemQuantity = (decimal?)(from ab in db.PEItemss
													 join ba in db.PurchaseEntrys on ab.PurchaseEntry equals ba.PurchaseEntryId
													 where ab.Item == a.Item && ba.PurType == PurchaseHireType.CrossHire
													 && ba.PurchaseEntryId == g.Invoice
													 && ab.itemNote != "-:{Bundle_Item}"
													 select new
													 {
														 ab.ItemQuantity
													 }).Sum(a => a.ItemQuantity) ?? 0,

					 }).ToList();

			var data = (from o in v
						select new
						{
							o.Item,
							o.ItemQuantity,
							o.ItemUnit,
							o.ItemUnitPrice,

							o.ItemDiscount,
							o.ItemID,
							o.ItemCode,
							o.ItemName,
							o.ItemWithCode,
							o.ItemUnitID,
							o.SubUnitId,
							o.note,
							PriUnit = (o.PriUnit != null) ? o.PriUnit : "",
							SubUnit = (o.SubUnit != null) ? o.SubUnit : "",
							o.categoryname,
							o.Tax,
							OpeningStock = o.OpeningStock,
							MinStock = (o.MinStock != null) ? o.MinStock : 0,
							o.ConFactor,
							o.SellingPrice,
							o.PurchasePrice,
							o.BasePrice,
							o.MRP,
							price = (o.SellingPrice != 0) ? o.SellingPrice : o.MRP,
							o.KeepStock,

							RetItemQuantity = o.RetItemQuantity,
							DvItemQuantity = o.DvItemQuantity,
							id = o.Item,
							text = o.ItemCode + " - " + o.ItemName,
							balance = o.DvItemQuantity - o.RetItemQuantity,

							o.ReceivedQty,
							o.DamageQty,
							o.MissingQty,

							bundle = (from ab in db.CrossHrItems
									  join fb in db.BundleItems on ab.Item equals fb.ItemId into bunit
									  from fb in bunit.DefaultIfEmpty()
									  join ay in db.ItemBundles on fb.ItemBundle equals ay.ItemBundleId into bun
									  from ay in bun.DefaultIfEmpty()
									  join bb in db.Items on ab.Item equals bb.ItemID
									  join dd in db.Scaffolds on bb.ItemID equals dd.Item into scaffold
									  from dd in scaffold.DefaultIfEmpty()
									  join eb in db.ItemUnits on fb.ItemUnit equals eb.ItemUnitID into bpunit
									  from eb in bpunit.DefaultIfEmpty()
									  join zz in db.CrossHireReturns on ab.Hr equals zz.HireReturnId
									  let bimg = db.ItemImages.Where(bim => bim.ItemID == ab.Item).Select(bim => new { bim.FileName, bim.Status, bim.ItemImageID }).ToList()
									  where ab.Hr == entryId && ab.ItemNote == "-:{Bundle_Item}"
									  && o.ItemID == ab.ItemDiscount && ay.mainItem == ab.ItemDiscount
									  select new
									  {
										  Id = bb.ItemID,
										  ItemUnitPrice = ab.ItemUnitPrice,
										  ItemQuantity = ab.ItemQuantity,
										  ItemDiscount = ab.ItemDiscount,
										  ItemNote = "",

										  ItemCode = bb.ItemCode,
										  ItemName = bb.ItemName,
										  ItemUnit = eb.ItemUnitName,
										  PartNumber = bb.PartNumber,
										  PNoStatus = "",
										  CBM = dd.CBM,
										  Weight = dd.Weight,
										  img = bimg,
										  note = ab.ItemNote.Replace("<br />", "\n"),

										  KeepStock = bb.KeepStock,

										  ab.Item,
										  ItemWithCode = bb.ItemCode + " - " + bb.ItemName,
										  bb.ItemUnitID,
										  bb.SubUnitId,
										  bb.ItemArabic,
										  bb.ItemDescription,
										  BaseQty = (ab.ItemQuantity / o.ItemQuantity),

										  balance = o.DvItemQuantity - o.RetItemQuantity,
										  ab.ReceivedQty,
										  ab.DamageQty,
										  ab.MissingQty,

										  RetItemQuantity = (decimal?)(from xx in db.CrossHrItems
																	   join yy in db.CrossHireReturns on xx.Hr equals yy.HireReturnId
																	   where xx.ItemNote == "-:{Bundle_Item}"
																	   && yy.HireReturnId == entryId
																	   && ab.Item == xx.Item
																	   && xx.ItemDiscount == ab.ItemDiscount
																	   select new
																	   {
																		   xx.ItemQuantity
																	   }).Sum(a => a.ItemQuantity) ?? 0,

										  DvItemQuantity = (decimal?)(from xx in db.PEItemss
																	  join yy in db.PurchaseEntrys on xx.PurchaseEntry equals yy.PurchaseEntryId
																	  where xx.itemNote == "-:{Bundle_Item}"
																	  && yy.PurchaseEntryId == zz.Invoice
																	  && ab.Item == xx.Item
																	  && xx.ItemDiscount == ab.ItemDiscount
																	  select new
																	  {
																		  xx.ItemQuantity
																	  }).Sum(a => a.ItemQuantity) ?? 0,


									  }).ToList(),



						}).ToList();
			return Json(data);
		}

		[HttpPost]
		public JsonResult GetHrByIds(long[] array)//long[] array 
		{
			var result = (from a in db.CrossHrItems
						  join b in db.Items on a.Item equals b.ItemID
						  join c in db.ItemUnits on b.ItemUnitID equals c.ItemUnitID into primary
						  from c in primary.DefaultIfEmpty()
						  join d in db.ItemUnits on b.SubUnitId equals d.ItemUnitID into second
						  from d in second.DefaultIfEmpty()
						  where array.Contains(a.Hr)
						  select new
						  {
							  b.ItemID,
							  a.Item,
							  a.ItemQuantity,
							  a.ItemUnit,
							  a.ItemUnitPrice,
							  a.ItemDiscount,
							  ItemCode = b.ItemCode,
							  note = a.ItemNote.Replace("<br />", "\n"),
							  ItemName = b.ItemName,
							  ItemWithCode = b.ItemCode + " - " + b.ItemName,
							  b.ItemUnitID,
							  b.SubUnitId,
							  PriUnit = c.ItemUnitName,
							  SubUnit = d.ItemUnitName,
							  b.BasePrice,
							  b.SellingPrice,
							  b.PurchasePrice,
							  b.MRP,
							  a.Hr
						  });
			return Json(result);
		}

		private Boolean DeleteHR(long sId)
		{
			var UserId = User.Identity.GetUserId();
			CrossHireReturn note = db.CrossHireReturns.Find(sId);

			var item = db.CrossHrItems.Where(a => a.Hr == sId).FirstOrDefault();
			if (item != null)
			{
				db.CrossHrItems.RemoveRange(db.CrossHrItems.Where(a => a.Hr == sId));
			}

			var appr = db.Approvals.Where(a => a.TransEntry == sId && a.Type == "CrossHireReturn").FirstOrDefault();
			if (appr != null)
			{
				db.Approvals.RemoveRange(db.Approvals.Where(a => a.TransEntry == sId && a.Type == "CrossHireReturn"));
			}
			var app = db.ApprovalUpdates.Where(a => a.TransEntry == sId && a.Type == "CrossHireReturn").FirstOrDefault();
			if (app != null)
			{
				db.ApprovalUpdates.RemoveRange(db.ApprovalUpdates.Where(a => a.TransEntry == sId && a.Type == "CrossHireReturn"));
			}
			db.CrossHireReturns.Remove(note);
			db.SaveChanges();
			com.addlog(LogTypes.Deleted, UserId, "CrossHireReturn", "CrossHireReturns", findip(), note.HireReturnId, "Successfully Deleted CrossHireReturn");
			return true;
		}

		private string InvoiceNo(Int64 HrNo = 0, string billNo = null)
		{
			var companyPrefix = db.CodePrefixs.Where(a => a.section == "CrossHireReturn").Select(a => a.prefix).FirstOrDefault();
			Int32 number = db.CodePrefixs.Where(a => a.section == "CrossHireReturn").Select(a => a.number).FirstOrDefault();
			if (billNo == null)
			{
				if ((db.CrossHireReturns.Select(p => p.HrNo).AsEnumerable().DefaultIfEmpty(0).Max()) == 0)
				{
					billNo = (number == 0) ? (companyPrefix + 1) : (companyPrefix + number);
				}
				else
				{
					HrNo = db.CrossHireReturns.Max(p => p.HrNo + 1);
					billNo = companyPrefix + HrNo;
					if (BillExist(billNo))
					{
						billNo = InvoiceNo(HrNo, billNo);
					}
				}
			}
			else
			{
				HrNo = HrNo + 1;
				billNo = companyPrefix + HrNo;
				if (BillExist(billNo))
				{
					billNo = InvoiceNo(HrNo, billNo);
				}
			}
			return billNo;
		}

		private bool BillExist(string No)
		{
			var Exists = db.CrossHireReturns.Any(c => c.BillNo == No);
			bool res = (Exists) ? true : false;
			return res;
		}

		private long GetNo()
		{
			Int64 No = 0;
			Int32 number = db.CodePrefixs.Where(a => a.section == "CrossHireReturn").Select(a => a.number).FirstOrDefault();
			if ((db.CrossHireReturns.Select(p => p.HrNo).AsEnumerable().DefaultIfEmpty(0).Max()) == 0)
			{
				if (number == 0)
				{
					No = 1;
				}
				else
				{
					No = number;
				}
			}
			else
			{
				No = db.CrossHireReturns.Max(p => p.HrNo + 1);
			}

			return No;
		}

		[HttpPost]
		public ActionResult GetCrossHireType(long Invoice)
		{
			var Supplier = db.PurchaseEntrys.Where(x => x.PurchaseEntryId == Invoice).Select(x => x.Supplier).FirstOrDefault();
			var Hire = (from b in db.PurchaseEntrys
						join h in db.HireDetails on new { h1 = Invoice, h2 = "Purchase" }
						equals new { h1 = h.Reference, h2 = h.Section } into hir
						from h in hir.DefaultIfEmpty()
						join c in db.Suppliers on b.Supplier equals c.SupplierID into supp
						from c in supp.DefaultIfEmpty()
						join d in db.Contacts on c.Contact equals d.ContactID into cnt
						from d in cnt.DefaultIfEmpty()
						where b.PurchaseEntryId == Invoice
						select new
						{
							Hty = h.HireType
						}).FirstOrDefault();
			return Json(Hire);

		}

		public JsonResult SearchInvoiceItem(string q, string x, long? cust, long? mc, string constat, long? Invoice = 0)
		{

			var StockItemsPerm = User.IsInRole("List Stockable Items");

			var EnablePartNumber = db.EnableSettings.Where(a => a.EnableType == "PartNoInItem").FirstOrDefault();
			var PartNoCheck = EnablePartNumber != null ? EnablePartNumber.Status : Status.inactive;

			var check = (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q));
			var ItemSalesPrice = User.IsInRole("ItemSalesPrice");
			var ItemPurchasePrice = User.IsInRole("ItemPurchasePrice");
			var ItemlastSalesPrice = User.IsInRole("ItemlastSalesPrice");
			var ItemlastPurchasePrice = User.IsInRole("ItemlastPurchasePrice");

			var start = Request.Form.GetValues("page").FirstOrDefault();
			int pageSize = 10;
			int skip = start != null ? (Convert.ToInt32(start) - 1) * 10 : 0;

			var item = (from i in db.PEItemss
						join j in db.PurchaseEntrys on i.PurchaseEntry equals j.PurchaseEntryId
						join b in db.Items on i.Item equals b.ItemID
						join c in db.ItemUnits on b.ItemUnitID equals c.ItemUnitID into primary
						from c in primary.DefaultIfEmpty()
						join d in db.ItemUnits on b.SubUnitId equals d.ItemUnitID into second
						from d in second.DefaultIfEmpty()
						join e in db.ItemCategorys on b.ItemCategoryID equals e.ItemCategoryID into cat
						from e in cat.DefaultIfEmpty()
						join f in db.Taxs on b.TaxID equals f.TaxID into taxss
						from f in taxss.DefaultIfEmpty()
							//equals new { f1 = g.Item, f2 = g.PEItemsId } into pur
							//equals new { f1 = h.Item, f2 = h.SEItemsId } into sale
						where (b.Status == Status.active && (check == true || b.ItemName.ToLower().Contains(q.ToLower()) || b.ItemCode.ToLower().Contains(q.ToLower()) || b.Barcode.ToLower().Contains(q.ToLower()) || b.PartNumber.ToLower().Contains(q.ToLower()) || b.ItemName.Contains(q) || b.ItemCode.Contains(q) || b.Barcode.Contains(q) || (b.PartNumber.Contains(q) && PartNoCheck == Status.active)))
						&& (i.PurchaseEntry == Invoice)
						select new
						{
							text = b.ItemCode + " - " + b.ItemName,
							id = b.ItemID,
							b.SellingPrice,
							b.PurchasePrice,
							b.BasePrice,
							b.MRP,
							b.MinStock,
							b.KeepStock,
							Tax = f.Percentage,
							b.ItemUnitID,

							PartNumber = (b.PartNumber != null && b.PartNumber != "") ? b.PartNumber : "",

							lastSale = (decimal?)null,
							lastSaleU = (decimal?)null,

							lastPur = (decimal?)null,
							lastPurU = (decimal?)null,

							b.ItemCode,
							b.ItemName,
							b.ItemArabic,
							b.SubUnitId,
							PriUnit = c.ItemUnitName,
							SubUnit = d.ItemUnitName,
							ConFactor = b.ConFactor != 0 ? b.ConFactor : 1,

							b.Barcode,

							OpeningStock = b.OpeningStock != null ? b.OpeningStock : 0,

							PriPurchase = (decimal?)(from v in db.PEItemss
													 join w in db.PurchaseEntrys on v.PurchaseEntry equals w.PurchaseEntryId
													 where v.Item == b.ItemID && v.ItemUnit == b.ItemUnitID
													 && (mc == 0 || mc == w.MaterialCenter)
													 select new
													 {
														 v.ItemQuantity
													 }).Sum(c => c.ItemQuantity) ?? 0,

							SubPurchase = (decimal?)(from v in db.PEItemss
													 join w in db.PurchaseEntrys on v.PurchaseEntry equals w.PurchaseEntryId
													 where v.Item == b.ItemID && v.ItemUnit == b.SubUnitId && c.ItemUnitName != d.ItemUnitName
													 && (mc == 0 || mc == w.MaterialCenter)
													 select new
													 {
														 v.ItemQuantity
													 }).Sum(c => c.ItemQuantity) ?? 0,

							PriSale = (decimal?)(from v in db.PEItemss
												 join w in db.PurchaseEntrys on v.PurchaseEntry equals w.PurchaseEntryId
												 where v.Item == b.ItemID && v.ItemUnit == b.ItemUnitID
												 && (mc == 0 || mc == w.MaterialCenter)
												 select new
												 {
													 v.ItemQuantity
												 }).Sum(c => c.ItemQuantity) ?? 0,

							SubSale = (decimal?)(from v in db.PEItemss
												 join w in db.PurchaseEntrys on v.PurchaseEntry equals w.PurchaseEntryId
												 where v.Item == b.ItemID && v.ItemUnit == b.SubUnitId && c.ItemUnitName != d.ItemUnitName
												 && (mc == 0 || mc == w.MaterialCenter)
												 select new
												 {
													 v.ItemQuantity
												 }).Sum(c => c.ItemQuantity) ?? 0,

							PriPReturn = (decimal?)(from v in db.PRItemss
													join w in db.PurchaseReturns on v.PurchaseReturnId equals w.purchaseEntryId
													where v.Item == b.ItemID && v.ItemUnit == b.ItemUnitID
													&& (mc == 0 || mc == w.MaterialCenter)
													select new
													{
														v.ItemQuantity
													}).Sum(c => c.ItemQuantity) ?? 0,

							SubPReturn = (decimal?)(from v in db.PRItemss
													join w in db.PurchaseReturns on v.PurchaseReturnId equals w.purchaseEntryId
													where v.Item == b.ItemID && v.ItemUnit == b.SubUnitId && c.ItemUnitName != d.ItemUnitName
													&& (mc == 0 || mc == w.MaterialCenter)
													select new
													{
														v.ItemQuantity
													}).Sum(c => c.ItemQuantity) ?? 0,

							PriSReturn = (decimal?)(from v in db.SRItemss
													join w in db.SalesReturns on v.SalesReturnId equals w.SalesReturnId
													where v.Item == b.ItemID && v.ItemUnit == b.ItemUnitID
													&& (mc == 0 || mc == w.MaterialCenter)
													select new
													{
														v.ItemQuantity
													}).Sum(c => c.ItemQuantity) ?? 0,

							SubSReturn = (decimal?)(from v in db.SRItemss
													join w in db.SalesReturns on v.SalesReturnId equals w.SalesReturnId
													where v.Item == b.ItemID && v.ItemUnit == b.SubUnitId && c.ItemUnitName != d.ItemUnitName
													&& (mc == 0 || mc == w.MaterialCenter)
													select new
													{
														v.ItemQuantity
													}).Sum(c => c.ItemQuantity) ?? 0,

							//stock adjustment---
							PriAddAdj = (decimal?)db.StockAdjustments.Where(a => a.ItemID == b.ItemID && a.ItemUnitID == b.ItemUnitID && a.AdjustmentType == AdjustmentType.Add).Select(a => a.ItemQuantity).Sum() ?? 0,
							SubAddAdj = (decimal?)db.StockAdjustments.Where(a => a.ItemID == b.ItemID && a.ItemUnitID == b.SubUnitId && c.ItemUnitName != d.ItemUnitName && a.AdjustmentType == AdjustmentType.Add).Select(a => a.ItemQuantity).Sum() ?? 0,

							PriLessAdj = (decimal?)db.StockAdjustments.Where(a => a.ItemID == b.ItemID && a.ItemUnitID == b.ItemUnitID && a.AdjustmentType == AdjustmentType.Less).Select(a => a.ItemQuantity).Sum() ?? 0,
							subLessAdj = (decimal?)db.StockAdjustments.Where(a => a.ItemID == b.ItemID && a.ItemUnitID == b.SubUnitId && c.ItemUnitName != d.ItemUnitName && a.AdjustmentType == AdjustmentType.Less).Select(a => a.ItemQuantity).Sum() ?? 0,
							//-------
							// production ----
							//PriProdItem = (decimal?)db.Productions.Where(a => (a.MaterialCenter == mc || mc == 0) && a.Item == b.ItemID && a.Unit == b.ItemUnitID).Select(a => a.Qty).Sum() ?? 0,
							//SubProdItem = (decimal?)db.Productions.Where(a => (a.MaterialCenter == mc || mc == 0) && a.Item == b.ItemID && a.Unit == b.SubUnitId && c.ItemUnitName != d.ItemUnitName).Select(a => a.Qty).Sum() ?? 0,
							PriProdItem = (decimal?)(from v in db.GeneratedItem
													 join w in db.Productions on v.Production equals w.ProductionId
													 where v.Item == b.ItemID && v.Unit == b.ItemUnitID
													 && (mc == 0 || mc == w.MaterialCenter)
													 select new
													 {
														 v.Qty
													 }).Sum(c => c.Qty) ?? 0,
							SubProdItem = (decimal?)(from v in db.GeneratedItem
													 join w in db.Productions on v.Production equals w.ProductionId
													 where v.Item == b.ItemID && v.Unit == b.SubUnitId && c.ItemUnitName != d.ItemUnitName
													 && (mc == 0 || mc == w.MaterialCenter)
													 select new
													 {
														 v.Qty
													 }).Sum(c => c.Qty) ?? 0,
							// compined item
							PriProdCItem = (decimal?)(from v in db.ProItems
													  join w in db.Productions on v.Production equals w.ProductionId
													  where v.ItemId == b.ItemID && v.Unit == b.ItemUnitID
													  && (mc == 0 || mc == w.MaterialCenter)
													  select new
													  {
														  v.Quantity
													  }).Sum(c => c.Quantity) ?? 0,

							SubProdCItem = (decimal?)(from v in db.ProItems
													  join w in db.Productions on v.Production equals w.ProductionId
													  where v.ItemId == b.ItemID && v.Unit == b.SubUnitId && c.ItemUnitName != d.ItemUnitName
													  && (mc == 0 || mc == w.MaterialCenter)
													  select new
													  {
														  v.Quantity
													  }).Sum(c => c.Quantity) ?? 0,

							// main item
							//PriUnItem = (decimal?)db.Unassembles.Where(a => (a.MaterialCenter == mc || mc == 0) && a.Item == b.ItemID && a.Unit == b.ItemUnitID).Select(a => a.Qty).Sum() ?? 0,
							//SubUnItem = (decimal?)db.Unassembles.Where(a => (a.MaterialCenter == mc || mc == 0) && a.Item == b.ItemID && a.Unit == b.SubUnitId && c.ItemUnitName != d.ItemUnitName).Select(a => a.Qty).Sum() ?? 0,
							PriUnItem = (decimal?)(from v in db.ConsumedItem
												   join w in db.Unassembles on v.Unassemble equals w.UnassembleId
												   where v.Item == b.ItemID && v.Unit == b.ItemUnitID
												   && (mc == 0 || mc == w.MaterialCenter)
												   select new
												   {
													   v.Qty
												   }).Sum(c => c.Qty) ?? 0,
							SubUnItem = (decimal?)(from v in db.ConsumedItem
												   join w in db.Unassembles on v.Unassemble equals w.UnassembleId
												   where v.Item == b.ItemID && v.Unit == b.SubUnitId && c.ItemUnitName != d.ItemUnitName
													 && (mc == 0 || mc == w.MaterialCenter)
												   select new
												   {
													   v.Qty
												   }).Sum(c => c.Qty) ?? 0,
							// compined item
							PriUnCItem = (decimal?)(from v in db.UnassembleItems
													join w in db.Unassembles on v.Unassemble equals w.UnassembleId
													where v.ItemId == b.ItemID && v.Unit == b.ItemUnitID
													&& (mc == 0 || mc == w.MaterialCenter)
													select new
													{
														v.Quantity
													}).Sum(c => c.Quantity) ?? 0,

							SubUnCItem = (decimal?)(from v in db.UnassembleItems
													join w in db.Unassembles on v.Unassemble equals w.UnassembleId
													where v.ItemId == b.ItemID && v.Unit == b.SubUnitId && c.ItemUnitName != d.ItemUnitName
													&& (mc == 0 || mc == w.MaterialCenter)
													select new
													{
														v.Quantity
													}).Sum(c => c.Quantity) ?? 0,


						}).Distinct();
			var vd = item.OrderBy(b => b.ItemName).Skip(skip).Take(pageSize).ToList();
			if (constat != "PurchaseEntry" || StockItemsPerm != true)
			{

			}
			else
			{
				vd = item.OrderBy(b => b.ItemName).ToList();
			}
			var data = vd.Select(o => new
			{
				o.text,
				o.id,
				unit = o.ItemUnitID,
				price = (ItemSalesPrice == true) ? (o.SellingPrice != 0) ? o.SellingPrice : o.MRP : (decimal?)null,
				cost = (ItemPurchasePrice == true) ? o.PurchasePrice : (decimal?)null,
				o.KeepStock,
				MinStock = (o.MinStock != null) ? o.MinStock : 0,
				o.Tax,
				lastSale = (ItemlastSalesPrice == true) ? o.lastSale : (decimal?)null,
				lastSaleU = o.lastSaleU,
				lastPur = (ItemlastPurchasePrice == true) ? o.lastPur : (decimal?)null,
				lastPurU = o.lastPurU,

				o.ItemCode,
				o.ItemName,
				o.ItemArabic,
				o.Barcode,
				o.SubUnitId,
				PartNumber = o.PartNumber,
				PriUnit = (o.PriUnit != null) ? o.PriUnit : "",
				SubUnit = (o.SubUnit != null) ? o.SubUnit : "",
				o.ConFactor,
				//o.SellingPrice,
				//o.PurchasePrice,
				//o.BasePrice,
				//o.MRP,

				pritotal = ((o.OpeningStock + o.PriPurchase + o.PriSReturn + o.PriAddAdj + o.PriProdItem + o.PriUnCItem) - (o.PriSale + o.PriPReturn + o.PriLessAdj + o.PriProdCItem + o.PriUnItem)),
				subtotal = ((o.SubPurchase + o.SubSReturn + o.SubAddAdj + o.SubProdItem + o.SubUnCItem) - (o.SubSale + o.SubPReturn + o.subLessAdj + o.SubProdCItem + o.SubUnItem)),
				total = (((o.OpeningStock + o.PriPurchase + o.PriSReturn + o.PriAddAdj + o.PriProdItem + o.PriUnCItem) - (o.PriSale + o.PriPReturn + o.PriLessAdj + o.PriProdCItem + o.PriUnItem)) * o.ConFactor) + ((o.SubPurchase + o.SubSReturn + o.SubAddAdj + o.SubProdItem + o.SubUnCItem) - (o.SubSale + o.SubPReturn + o.subLessAdj + o.SubProdCItem + o.SubUnItem)),
				cust = cust,
			}).OrderBy(b => b.text).ToList();
			//&& (constat != "PurchaseEntry" || (StockItemsPerm != true || b.KeepStock == true || b.OpeningStock > 0 ))
			if (constat == "PurchaseEntry" && StockItemsPerm == true)
			{
				data = data.Where(a => a.KeepStock == true && a.total > 0).Skip(skip).Take(pageSize).ToList();
			}

			return Json(data);
		}

		public JsonResult SearchHireItem(string q, string x, long? pentry, long? qrystr)
		{

			var data = (from a in db.PEItemss
						join b in db.Items on a.Item equals b.ItemID
						join c in db.ItemUnits on b.ItemUnitID equals c.ItemUnitID into primary
						from c in primary.DefaultIfEmpty()
						join d in db.ItemUnits on b.SubUnitId equals d.ItemUnitID into second
						from d in second.DefaultIfEmpty()
						join e in db.ItemCategorys on b.ItemCategoryID equals e.ItemCategoryID into cat
						from e in cat.DefaultIfEmpty()
						join f in db.Taxs on b.TaxID equals f.TaxID into taxss
						from f in taxss.DefaultIfEmpty()
						join g in db.PurchaseEntrys on a.PurchaseEntry equals g.PurchaseEntryId
						where a.PurchaseEntry == pentry && a.itemNote != "-:{Bundle_Item}"
						&& g.PurType == PurchaseHireType.CrossHire //&& b.KeepStock == true
						select new
						{
							a.Item,
							a.ItemQuantity,
							a.ItemUnit,
							a.ItemUnitPrice,
							a.ItemTax,
							a.ItemSubTotal,
							a.ItemTaxAmount,
							a.ItemDiscount,
							a.ItemTotalAmount,
							note = a.itemNote.Replace("<br />", "\n"),

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
							categoryname = e.ItemCategoryName,
							Tax = f.Percentage,
							b.SellingPrice,
							b.PurchasePrice,
							b.BasePrice,
							b.MRP,
							b.KeepStock,

							RetItemQuantity = (decimal?)(from aa in db.CrossHireReturns
														 join bb in db.CrossHrItems on aa.HireReturnId equals bb.Hr
														 where bb.Item == a.Item && aa.Invoice == pentry
														 && bb.ItemNote != "-:{Bundle_Item}"
														 select new
														 {
															 bb.ItemQuantity
														 }).Sum(a => a.ItemQuantity) ?? 0,

							DvItemQuantity = (decimal?)(from ab in db.PurchaseEntrys
														join ba in db.PEItemss on ab.PurchaseEntryId equals ba.PurchaseEntry
														where ba.Item == a.Item && ab.PurType == PurchaseHireType.CrossHire
														&& ab.PurchaseEntryId == pentry
														&& ba.itemNote != "-:{Bundle_Item}"
														select new
														{
															ba.ItemQuantity
														}).Sum(a => a.ItemQuantity) ?? 0,
						}).AsEnumerable().Select(o => new
						{
							o.Item,
							o.ItemQuantity,
							o.ItemUnit,
							o.ItemUnitPrice,
							o.ItemTax,
							o.ItemSubTotal,
							o.ItemTaxAmount,
							o.ItemDiscount,
							o.ItemTotalAmount,
							o.ItemID,
							o.ItemCode,
							o.ItemName,
							o.ItemWithCode,
							o.ItemUnitID,
							o.SubUnitId,
							o.note,
							PriUnit = (o.PriUnit != null) ? o.PriUnit : "",
							SubUnit = (o.SubUnit != null) ? o.SubUnit : "",
							o.categoryname,
							o.Tax,
							OpeningStock = o.OpeningStock,
							MinStock = (o.MinStock != null) ? o.MinStock : 0,
							o.ConFactor,
							o.SellingPrice,
							o.PurchasePrice,
							o.BasePrice,
							o.MRP,
							price = (o.SellingPrice != 0) ? o.SellingPrice : o.MRP,
							o.KeepStock,

							RetItemQuantity = o.RetItemQuantity,
							DvItemQuantity = o.DvItemQuantity,
							id = o.Item,
							text = o.ItemCode + " - " + o.ItemName,
							balance = o.DvItemQuantity - o.RetItemQuantity
						}).ToList();

			if (qrystr == null)
			{
				data = data.Where(a => a.balance > 0).ToList();
			}
			else
			{
				var invoice = db.CrossHrItems.Where(a => a.Hr == qrystr).Select(a => a.Item).ToArray();
				data = data.Where(a => invoice.Contains(a.Item)).ToList();
			}
			return Json(data);
		}


		public JsonResult SearchHireItemById(long? entryId, long? ItemId)
		{

			var v = (from a in db.PEItemss
					 join b in db.Items on a.Item equals b.ItemID
					 join c in db.ItemUnits on b.ItemUnitID equals c.ItemUnitID into primary
					 from c in primary.DefaultIfEmpty()
					 join d in db.ItemUnits on b.SubUnitId equals d.ItemUnitID into second
					 from d in second.DefaultIfEmpty()
					 join e in db.ItemCategorys on b.ItemCategoryID equals e.ItemCategoryID into cat
					 from e in cat.DefaultIfEmpty()
					 join f in db.Taxs on b.TaxID equals f.TaxID into taxss
					 from f in taxss.DefaultIfEmpty()
					 join g in db.PurchaseEntrys on a.PurchaseEntry equals g.PurchaseEntryId
					 where a.PurchaseEntry == entryId && a.itemNote != "-:{Bundle_Item}"
					 && g.PurType == PurchaseHireType.CrossHire && a.Item == ItemId //&& b.KeepStock == true
					 select new
					 {
						 a.Item,
						 a.ItemQuantity,
						 a.ItemUnit,
						 a.ItemUnitPrice,
						 a.ItemTax,
						 a.ItemSubTotal,
						 a.ItemTaxAmount,
						 a.ItemDiscount,
						 a.ItemTotalAmount,
						 note = a.itemNote.Replace("<br />", "\n"),

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
						 categoryname = e.ItemCategoryName,
						 Tax = f.Percentage,
						 b.SellingPrice,
						 b.PurchasePrice,
						 b.BasePrice,
						 b.MRP,
						 b.KeepStock,

						 RetItemQuantity = (decimal?)(from aa in db.CrossHireReturns
													  join bb in db.CrossHrItems on aa.HireReturnId equals bb.Hr
													  where bb.Item == a.Item && aa.Invoice == entryId
													  && bb.ItemNote != "-:{Bundle_Item}"
													  select new
													  {
														  bb.ItemQuantity
													  }).Sum(a => a.ItemQuantity) ?? 0,

						 DvItemQuantity = (decimal?)(from ab in db.PurchaseEntrys
													 join ba in db.PEItemss on ab.PurchaseEntryId equals ba.PurchaseEntry
													 where ba.Item == a.Item && ab.PurType == PurchaseHireType.CrossHire
													 && ab.PurchaseEntryId == entryId
													 && ba.itemNote != "-:{Bundle_Item}"
													 select new
													 {
														 ba.ItemQuantity
													 }).Sum(a => a.ItemQuantity) ?? 0,

					 }).ToList();

			var data = (from o in v
						select new
						{
							o.Item,
							o.ItemQuantity,
							o.ItemUnit,
							o.ItemUnitPrice,
							o.ItemTax,
							o.ItemSubTotal,
							o.ItemTaxAmount,
							o.ItemDiscount,
							o.ItemTotalAmount,
							o.ItemID,
							o.ItemCode,
							o.ItemName,
							o.ItemWithCode,
							o.ItemUnitID,
							o.SubUnitId,
							o.note,
							PriUnit = (o.PriUnit != null) ? o.PriUnit : "",
							SubUnit = (o.SubUnit != null) ? o.SubUnit : "",
							o.categoryname,
							o.Tax,
							OpeningStock = o.OpeningStock,
							MinStock = (o.MinStock != null) ? o.MinStock : 0,
							o.ConFactor,
							o.SellingPrice,
							o.PurchasePrice,
							o.BasePrice,
							o.MRP,
							price = (o.SellingPrice != 0) ? o.SellingPrice : o.MRP,
							o.KeepStock,

							RetItemQuantity = o.RetItemQuantity,
							DvItemQuantity = o.DvItemQuantity,
							id = o.Item,
							text = o.ItemCode + " - " + o.ItemName,
							balance = o.DvItemQuantity - o.RetItemQuantity,

							bundle = (from ab in db.PEItemss
									  join bb in db.Items on ab.Item equals bb.ItemID
									  join dd in db.Scaffolds on bb.ItemID equals dd.Item into scaffold
									  from dd in scaffold.DefaultIfEmpty()

									  join eb in db.ItemUnits on ab.ItemUnit equals eb.ItemUnitID into bpunit
									  from eb in bpunit.DefaultIfEmpty()

									  let bimg = db.ItemImages.Where(bim => bim.ItemID == ab.Item).Select(bim => new { bim.FileName, bim.Status, bim.ItemImageID }).ToList()
									  where ab.PurchaseEntry == entryId && ab.itemNote == "-:{Bundle_Item}"
									  && o.ItemID == ab.ItemDiscount
									  select new
									  {
										  Id = bb.ItemID,
										  ItemUnitPrice = ab.ItemUnitPrice,
										  ItemQuantity = ab.ItemQuantity,
										  ItemSubTotal = ab.ItemSubTotal,
										  ItemDiscount = ab.ItemDiscount,
										  ItemNote = "",
										  ItemTax = ab.ItemTax,
										  ItemTaxAmount = ab.ItemTaxAmount,
										  ItemTotalAmount = ab.ItemTotalAmount,

										  ItemCode = bb.ItemCode,
										  ItemName = bb.ItemName,
										  ItemUnit = eb.ItemUnitName,
										  //ItemUnitName = eb.ItemUnit
										  PartNumber = bb.PartNumber,
										  PNoStatus = "",
										  CBM = dd.CBM,
										  Weight = dd.Weight,
										  img = bimg,
										  note = ab.itemNote.Replace("<br />", "\n"),

										  KeepStock = bb.KeepStock,

										  ab.Item,
										  ItemWithCode = bb.ItemCode + " - " + bb.ItemName,
										  bb.ItemUnitID,
										  bb.SubUnitId,
										  bb.ItemArabic,
										  bb.ItemDescription,
										  BaseQty = (ab.ItemQuantity / o.ItemQuantity),
										  RetItemQuantity = (decimal?)(from xx in db.CrossHrItems
																	   join yy in db.CrossHireReturns on xx.Hr equals yy.HireReturnId
																	   where xx.ItemNote == "-:{Bundle_Item}"
																	   && yy.Invoice == entryId
																	   && ab.Item == xx.Item
																	   && xx.ItemDiscount == ab.ItemDiscount
																	   select new
																	   {
																		   xx.ItemQuantity
																	   }).Sum(a => a.ItemQuantity) ?? 0,

										  DvItemQuantity = (decimal?)(from xx in db.PEItemss
																	  join yy in db.PurchaseEntrys on xx.PurchaseEntry equals yy.PurchaseEntryId
																	  where xx.itemNote == "-:{Bundle_Item}"
																	  && yy.PurchaseEntryId == entryId
																	  && ab.Item == xx.Item
																	  && xx.ItemDiscount == ab.ItemDiscount
																	  select new
																	  {
																		  xx.ItemQuantity
																	  }).Sum(a => a.ItemQuantity) ?? 0,
									  }).ToList(),
						}).ToList();

			return Json(data);
		}




		public JsonResult SearchHireEntry(string q)
		{
			object serialisedJson;
			if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
			{

				serialisedJson = (from a in db.CrossHireReturns
								  join b in db.PurchaseEntrys on a.Invoice equals b.PurchaseEntryId
								  where a.BillNo.Contains(q) || a.BillNo.ToLower().Contains(q.ToLower())
                                  select new
								  {
									  id = b.PurchaseEntryId,
									  text = b.BillNo
								  }).OrderBy(b => b.text).ToList();
			}
			else
			{
				serialisedJson = (from a in db.CrossHireReturns
								  join b in db.PurchaseEntrys on a.Invoice equals b.PurchaseEntryId
								  select new
								  {
									  id = b.PurchaseEntryId,
									  text = b.BillNo
								  }).OrderBy(b => b.text).ToList();
			}
			return Json(serialisedJson);
		}
		[HttpGet]
		public ActionResult EditStatus(long id)
		{
			var UserId = User.Identity.GetUserId();
			var appstat = db.ApprovalUpdates.Where(x => x.TransEntry == id && x.Type == "CrossHireReturn" && x.ApprovedBy == UserId).GroupBy(l => l.ApprovedBy)
									   .Select(g => g.OrderByDescending(c => c.CreatedDate).FirstOrDefault())
									   .ToList();

			var Stat = from ApprovalStatus e in Enum.GetValues(typeof(ApprovalStatus))
					   where e != ApprovalStatus.PendingApproval && (appstat.Count == 0 || e != appstat.Select(a => a.ApprovalStatus).FirstOrDefault())
					   select new
					   {
						   ID = (int)e,
						   Name = e.ToString()
					   };
			ViewBag.AppStat = QkSelect.List(Stat, "Id", "Name");

			return PartialView();
		}

		[HttpPost]
		public ActionResult EditStatus(ApprovalUpdate App, long id)
		{
			bool stat = false;
			string msg = "";
			var UserId = User.Identity.GetUserId();

			var MR = db.CrossHireReturns.Where(a => a.HireReturnId == id).FirstOrDefault();

			var chkappby = db.ApprovalUpdates.Where(a => a.ApprovedBy == UserId && a.TransEntry == id && a.Type == "CrossHireReturn").OrderByDescending(a => a.CreatedDate).FirstOrDefault();
			if ((chkappby == null) || (chkappby.ApprovalStatus != App.ApprovalStatus))
			{
				ApprovalUpdate AppUp = new ApprovalUpdate();
				AppUp.ApprovalStatus = App.ApprovalStatus;
				AppUp.CreatedDate = Convert.ToDateTime(System.DateTime.Now);
				AppUp.ApprovedBy = UserId;
				AppUp.Note = App.Note;
				AppUp.RequestBy = MR.CreatedUserId;
				AppUp.Status = Status.active;
				AppUp.TransEntry = id;
				AppUp.Type = "CrossHireReturn";

				db.ApprovalUpdates.Add(AppUp);
				db.SaveChanges();

				stat = true;
				msg = "Successfully Updated Status.";
				return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
			}
			else
			{
				stat = false;
				msg = "Updating Same Status.";
				return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
			}

		}

		[HttpPost]
		public ActionResult GetAllStatusUpdation(long MCId)
		{
			var draw = Request.Form.GetValues("draw").FirstOrDefault();
			var start = Request.Form.GetValues("start").FirstOrDefault();
			var length = Request.Form.GetValues("length").FirstOrDefault();
			//Find Order Column
			var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
			var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();


			int pageSize = length != null ? Convert.ToInt32(length) : 0;
			int skip = start != null ? Convert.ToInt32(start) : 0;
			int recordsTotal = 0;
			var UserView = (from b in db.ApprovalUpdates
							join c in db.Users on b.ApprovedBy equals c.Id
							join d in db.CrossHireReturns on b.TransEntry equals d.HireReturnId into team
							from d in team.DefaultIfEmpty()
							join e in db.Employees on b.RequestBy equals e.UserId into emp
							from e in emp.DefaultIfEmpty()
							join u in db.Users on d.CreatedUserId equals u.Id into req
							from u in req.DefaultIfEmpty()
							where b.TransEntry == MCId && b.Type == "CrossHireReturn"
							select new
							{
								b.ApprovalUpdateID,
								b.TransEntry,
								b.Status,
								b.ApprovalStatus,
								b.CreatedDate,
								b.Note,
								RequestBy = u.UserName,
								c.UserName,
								ApprovedBy = "" //e.FirstName + " " + e.LastName,
							}).Distinct().ToList().Select(o => new
							{
								o.ApprovalUpdateID,
								o.TransEntry,
								o.Status,
								ApprovalStatus = Enum.GetName(typeof(ApprovalStatus), o.ApprovalStatus),

								o.ApprovedBy,
								o.RequestBy,
								User = o.UserName, //db.Users.Where(a => a.Id == o.CreatedUser).Select(a => a.UserName).FirstOrDefault(),
								o.CreatedDate,
								Remarks = o.Note
							});
			recordsTotal = UserView.Count();
			var data = UserView.Skip(skip).Take(pageSize).ToList();
			return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
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
