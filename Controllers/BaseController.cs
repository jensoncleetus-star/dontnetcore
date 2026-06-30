using System.Linq.Dynamic.Core;
using ApplicationUserManager = Microsoft.AspNetCore.Identity.UserManager<QuickSoft.Models.ApplicationUser>;
using ApplicationSignInManager = Microsoft.AspNetCore.Identity.SignInManager<QuickSoft.Models.ApplicationUser>;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using QuickSoft.Helpers;
using QuickSoft.Models;
using System.Linq;
using System;
using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace QuickSoft.Controllers
{
    public class BaseController : Controller
    {
        ApplicationDbContext db;
        // Legacy Session["key"] + Server.MapPath support for all controllers.
        public new LegacySession Session => new LegacySession(HttpContext.Session);
        public LegacyServer Server => new LegacyServer();
        public BaseController()
        {
            db = new ApplicationDbContext();

            
        }

        // Legacy-faithful JSON for projections System.Text.Json cannot serialize: several bill/item
        // pickers project member pairs differing only by CASE (e.g. `Type` and `type = b.BillType`).
        // MVC5's JavaScriptSerializer emitted both keys, but System.Text.Json throws
        // "The JSON property name ... collides with another property", which killed those endpoints
        // after the port. Newtonsoft + MicrosoftDateFormat reproduces the exact legacy wire format
        // (both keys, "/Date(ms)/" dates) without touching the projections or the views.
        private static readonly Newtonsoft.Json.JsonSerializerSettings LegacyJsonSettings = new Newtonsoft.Json.JsonSerializerSettings
        {
            DateFormatHandling = Newtonsoft.Json.DateFormatHandling.MicrosoftDateFormat
        };
        protected ContentResult LegacyJson(object data)
        {
            return Content(Newtonsoft.Json.JsonConvert.SerializeObject(data, LegacyJsonSettings), "application/json; charset=utf-8");
        }

        public void Success(string message, bool dismissable = false)
        {
            AddAlert(AlertStyles.Success, message, "Success !", AlertIcons.Success, dismissable);
        }

        public void Information(string message, bool dismissable = false)
        {
            AddAlert(AlertStyles.Information, message, "Info !", AlertIcons.Information, dismissable);
        }

        public void Warning(string message, bool dismissable = false)
        {
            AddAlert(AlertStyles.Warning, message, "Warning !", AlertIcons.Warning, dismissable);
        }

        public void Danger(string message, bool dismissable = false)
        {
            AddAlert(AlertStyles.Danger, message, "Error !", AlertIcons.Danger, dismissable);
        }

        private void AddAlert(string alertStyle, string message, string heading, string alertIcon, bool dismissable)
        {
            // Store the alerts as a JSON string. ASP.NET Core's TempData serializer only supports primitive
            // types, so a raw List<Alert> throws "DefaultTempDataSerializer cannot serialize ..." when the next
            // redirect persists TempData (MVC5's session-backed TempData allowed arbitrary objects). JSON round-trips.
            var alerts = TempData.ContainsKey(Alert.TempDataKey)
                ? (Newtonsoft.Json.JsonConvert.DeserializeObject<List<Alert>>((string)TempData[Alert.TempDataKey]) ?? new List<Alert>())
                : new List<Alert>();

            alerts.Add(new Alert
            {
                AlertStyle = alertStyle,
                Message = message,
                Heading = heading,
                AlertIcon = alertIcon,
                Dismissable = dismissable
            });

            TempData[Alert.TempDataKey] = Newtonsoft.Json.JsonConvert.SerializeObject(alerts);
        }
        // find ip

        public string findip()
        {
            string ipAddress = Request.Headers["X-Forwarded-For"].ToString();
            if (string.IsNullOrEmpty(ipAddress))
            {
                ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            }
            return ipAddress;
        }
        public void userDetails()
        {
        }
        // company details set to viewbag
        [HttpPost]
        public JsonResult AddReminderall(string Type, long Reference, string Note, string RDate, string RTime, long[] emp,string actionurl)
        {
            bool stat = false;
            string msg;
            Int64 Id = 0;
            var UserId = User.Identity.GetUserId();
            var today = Convert.ToDateTime(System.DateTime.Now);

            var created = "";
            if (Type == "Leads")
            {
                created = "";
            }
            if (Type == "Task")
            {
                created = db.ProTasks.Where(a => a.ProTaskId == Reference).Select(a => a.CreatedBy).FirstOrDefault();
            }
            if (Type == "Sale")
            {
                created = db.SalesEntrys.Where(a => a.SalesEntryId == Reference).Select(a => a.CreatedBy).FirstOrDefault();
            }
            if (Type == "Quot")
            {
                created = db.Quotations.Where(a => a.QuotationId == Reference).Select(a => a.CreatedUserId).FirstOrDefault();
            }
            if (Type == "SReturn")
            {
                created = db.SalesReturns.Where(a => a.SalesReturnId == Reference).Select(a => a.CreatedBy).FirstOrDefault();
            }
            if (Type == "SOrder")
            {
                created = db.SalesOrders.Where(a => a.SalesOrderId == Reference).Select(a => a.CreatedUserId).FirstOrDefault();
            }
            if (Type == "ProForma")
            {
                created = db.ProFormas.Where(a => a.ProFormaId == Reference).Select(a => a.CreatedBy).FirstOrDefault();
            }
            if (Type == "Purchase")
            {
                created = db.PurchaseEntrys.Where(a => a.PurchaseEntryId == Reference).Select(a => a.CreatedBy).FirstOrDefault();
            }
            if (Type == "PReturn")
            {
                created = db.PurchaseReturns.Where(a => a.PurchaseReturnId == Reference).Select(a => a.CreatedBy).FirstOrDefault();
            }
            if (Type == "POrder")
            {
                created = db.PurchaseOrders.Where(a => a.PurchaseOrderId == Reference).Select(a => a.CreatedUserId).FirstOrDefault();
            }
            if (Type == "PQuot")
            {
                created = db.PurchaseQuotations.Where(a => a.PQuotationId == Reference).Select(a => a.CreatedUserId).FirstOrDefault();
            }
            if (Type == "DVNote")
            {
                created = db.Deliverynotes.Where(a => a.DeliverynoteId == Reference).Select(a => a.CreatedUserId).FirstOrDefault();
            }
            if (Type == "HReturn")
            {
                created = db.HireReturns.Where(a => a.HireReturnId == Reference).Select(a => a.CreatedUserId).FirstOrDefault();
            }
            if (Type == "MReqn")
            {
                created = db.MaterialRequisitions.Where(a => a.MaterialRequisitionId == Reference).Select(a => a.CreatedUserId).FirstOrDefault();
            }
            if (Type == "MRNote")
            {
                created = db.MaterialReceiveNotes.Where(a => a.MRId == Reference).Select(a => a.CreatedUserId).FirstOrDefault();
            }
            if (Type == "JobCard")
            {
                created = db.JobCards.Where(a => a.JobCardId == Reference).Select(a => a.CreatedBy).FirstOrDefault();
            }
            if (Type == "PKList")
            {
                created = db.PackingLists.Where(a => a.PackinglistId == Reference).Select(a => a.CreatedBy).FirstOrDefault();
            }

            if (Type == "StkTrans")
            {
                created = db.StockTransfers.Where(a => a.Id == Reference).Select(a => a.CreatedBy).FirstOrDefault();
            }
            if (Type == "StkJnl")
            {
                created = db.StockJournals.Where(a => a.Id == Reference).Select(a => a.CreatedBy).FirstOrDefault();
            }
            if (Type == "Prod")
            {
                created = db.Productions.Where(a => a.ProductionId == Reference).Select(a => a.CreatedBy).FirstOrDefault();
            }
            if (Type == "Unass")
            {
                created = db.Unassembles.Where(a => a.UnassembleId == Reference).Select(a => a.CreatedBy).FirstOrDefault();
            }
            var s = db.Reminderss.Select(o => o.ReminderId).ToList();
            Reminderss reminds = new Reminderss();
            reminds.Reference = Reference;
            reminds.Note = Note;
            var rDate=System.DateTime.Now;
           
            if (!string.IsNullOrEmpty(RDate))
           rDate = DateTime.Parse(RDate.ToString(), new CultureInfo("en-GB"));
          

            DateTime date = rDate;


            reminds.Type = Type;
            reminds.RStatus = "Open";
            reminds.RequestBy = created;

            reminds.CreatedBy = UserId;
            reminds.Status =0;
            reminds.CreatedDate = today;
            reminds.actionurl = actionurl;
            db.Reminderss.Add(reminds);
            db.SaveChanges();
            Id = reminds.ReminderId;


            //Approved By

            ReminderAssignedss remAs = new ReminderAssignedss();
            foreach (var empid in emp)
            {
                remAs.ReminderId = Id;
                remAs.EntryId = Reference;
                remAs.Type = Type;
                remAs.EmployeeId = empid;
                db.ReminderAssignedss.Add(remAs);
                db.SaveChanges();
            }


            msg = "Reminder added successfully.";
            stat = true;
          
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg, Id = Id } };
        }
        public void companySet()
        {
            var cdetails = db.companys
                            .Select(s => new
                            {
                                CName = s.CPName,
                                CAddress = s.CPAddress,
                                CEmail = s.CPEmail,
                                CTaxRegNo = s.TRN,
                                CPhone = s.CPPhone,
                                s.CPMobile,
                                s.CPFax,
                                CLogo = s.CPLogo,
                            }).FirstOrDefault();
            ViewBag.CName = cdetails.CName;
            ViewBag.CAddress = cdetails.CAddress;
            ViewBag.CEmail = cdetails.CEmail;
            ViewBag.CTaxRegNo = cdetails.CTaxRegNo;
            ViewBag.CPhone = cdetails.CPhone;
            ViewBag.CPMob = cdetails.CPMobile;
            ViewBag.CPFax = cdetails.CPFax;
            ViewBag.CLogo = cdetails.CLogo;

            var comHead = db.CompanyHeaders.FirstOrDefault();
            ViewBag.Header = comHead.Header;
            ViewBag.Footer = comHead.Footer;
        }
        // Financial Year
        public void _FinancialYear()
        {
            DateTime? FStartDate = null;
            DateTime? FEndDate = null;

            var SDate = db.FinancialYears.Select(i => i.Start).FirstOrDefault();
            if (SDate != null)
            {
                HttpCookie Newcookie = Request.Cookies["FinYearID"];
                if (Newcookie != null)
                {
                    Int32 TempYearid = Convert.ToInt32(Newcookie.Value);
                    FStartDate = db.FinancialYears.Where(x => x.id == TempYearid).Select(i => i.Start).First();
                    FEndDate = db.FinancialYears.Where(x => x.id == TempYearid).Select(i => i.End).First();
                }
                else
                {
                    FStartDate = db.FinancialYears.Max(i => i.Start);
                    FEndDate = db.FinancialYears.Max(i => i.End);
                }
            }
            else
            {
                FStartDate = DateTime.Now.AddYears(-4);
                FEndDate = DateTime.Now.AddYears(3);
            }
            ViewBag.StartDate = ((DateTime)FStartDate).ToString("MM/dd/yyyy");
            ViewBag.EndDate = ((DateTime)FEndDate).ToString("MM/dd/yyyy");
        }
    }
    // redirect methord 
    // ref: https://stackoverflow.com/questions/4793452/mvc-redirect-inside-the-constructor
    public class RedirectingActionAttribute : ActionFilterAttribute
    {
        ApplicationDbContext db;
        public RedirectingActionAttribute()
        {
            db = new ApplicationDbContext();
        }
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (!filterContext.HttpContext.Request.IsAjaxRequest())
            {
                base.OnActionExecuting(filterContext);
                var qry = @"IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Accounts') SELECT 1 ELSE SELECT 0";
                int rslt = db.Database.SqlQueryRaw<int>(qry).AsEnumerable().FirstOrDefault();

                if (rslt == 1)
                {
                    var installed = db.SystemConfigs.Any();
                    if (!installed)
                    {
                        filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(new
                        {
                            controller = "Installation",
                            action = "Index"
                        }));
                    }
                    else
                    {
                        //app version
                        
                        ((Controller)filterContext.Controller).ViewBag.Versions= db.AppVersions.Select(a => a.Versions).FirstOrDefault();

                        var details = db.SystemConfigs.SingleOrDefault();
                        SystemConfig sys = details; /*db.Suppliers.Find(id);*/
                        var systemtype = (SystemType)Enum.Parse(typeof(SystemType), Security.Decrypt(details.SystemTypes, General.keyval));
                        var sdate = Security.Decrypt(details.StartDate, General.keyval);
                        var today = Convert.ToDateTime(System.DateTime.Now.ToShortDateString());
                        DateTime startDate;
                        DateTime lastDate;
                        String format = "dd-MM-yyyy";
                        try
                        {
                            startDate = DateTime.ParseExact(sdate, format, new CultureInfo("en-GB"));
                        }
                        catch
                        {
                            try
                            {
                                startDate = Convert.ToDateTime(sdate);
                            }
                            catch
                            {
                                startDate =today.AddMonths(-1);
                            }
                        }
                        if (!string.IsNullOrEmpty(details.sld) && !string.IsNullOrWhiteSpace(details.sld))
                        {
                            var sld = Security.Decrypt(details.sld, General.keyval);
                            try
                            {
                                lastDate = DateTime.ParseExact(sld, format, new CultureInfo("en-GB"));
                            }
                            catch
                            {
                                lastDate = Convert.ToDateTime(sld);
                            }
                            if ((today < startDate) && (lastDate > today))
                            {
                                filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(new
                                {
                                    controller = "Home",
                                    action = "DateError"
                                }));
                            }
                            else
                            {
                              
                                if (systemtype == SystemType.Demo)
                                {
                                    DateTime endDate = System.DateTime.Now;
                                    try
                                    {
                                         endDate = Convert.ToDateTime(Security.Decrypt(details.EndDate, General.keyval));
                                    }
                                    catch
                                    {
                                        endDate= DateTime.ParseExact(Security.Decrypt(details.EndDate, General.keyval), "M/dd/yyyy hh:mm:ss tt", new CultureInfo("en-US"));

                                    }
                                    var timeperiod = Convert.ToInt32(Security.Decrypt(details.Extentdays, General.keyval));
                                    DateTime newDate = startDate.AddDays(timeperiod);
                                    if (newDate < today)
                                    {
                                        filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(new
                                        {
                                            controller = "Home",
                                            action = "Expired"
                                        }));
                                    }
                                }
                                else
                                {
                                    var MainKey = details.LicenseKey;
                                    var SysKey = details.SystemCode;
                                    var RProductKey = Security.RevkEYgEN(SysKey);
                                    var RLiscence = Security.RevLicKey(MainKey);
                                    if (RProductKey != RLiscence || MainKey == null || SysKey == null)
                                    {
                                        filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(new
                                        {
                                            controller = "Home",
                                            action = "Error"
                                        }));
                                    }
                                }
                                var insdate = Security.Encrypt(System.DateTime.Now.ToString("dd-MM-yyyy").ToString(), General.keyval);
                                sys.sld = Convert.ToString(insdate);
                                db.Entry(sys).State = EntityState.Modified;
                                db.SaveChanges();
                            }
                        }
                        else
                        {
                            filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(new
                            {
                                controller = "Home",
                                action = "DateError"
                            }));
                        }

                    }
                }
                else
                {
                    filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(new
                    {
                        controller = "Installation",
                        action = "Index"
                    }));
                }
            }
        }
    }
    public class AllowCrossSiteJsonAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            filterContext.HttpContext.Response.Headers.Append("Access-Control-Allow-Origin", "*");
            base.OnActionExecuting(filterContext);
        }
    }
    // Ported from OWIN AuthorizeAttribute.HandleUnauthorizedRequest to a Core authorization filter.
    public class QkAuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        public string Roles { get; set; }

        public void OnAuthorization(AuthorizationFilterContext filterContext)
        {
            // Security S8: honor [AllowAnonymous] so a class-level [QkAuthorize] (and the global fallback policy)
            // never blocks an action that explicitly opted out (login, error pages, OTP downloads, installation).
            if (filterContext.HttpContext.GetEndpoint()?.Metadata.GetMetadata<IAllowAnonymous>() != null)
                return;

            var user = filterContext.HttpContext.User;
            if (user?.Identity == null || !user.Identity.IsAuthenticated)
            {
                // Not authenticated -> send to login
                filterContext.Result = new RedirectToActionResult("Login", "Users", null);
                return;
            }

            var roles = (this.Roles ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries);
            if (roles.Length > 0 && !roles.Any(r => user.IsInRole(r.Trim())))
            {
                var isAjax = filterContext.HttpContext.Request.Headers["X-Requested-With"].ToString() == "XMLHttpRequest";
                if (!isAjax)
                    filterContext.Result = new ViewResult { ViewName = "~/Views/Shared/Unauthorized.cshtml" };
                else
                    filterContext.Result = new RedirectToActionResult("Unauthorize", "Home", null);
            }
        }
    }
}
