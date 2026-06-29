using QuickSoft.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QuickSoft.Models;
using QuickSoft.Controllers;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace QuickSoft.Areas.Property.Controllers
{
    // Advanced Real-Estate analytics: per-property Profit & Loss, contractor allocation,
    // and unit-level insights. Self-contained (server-side computed, no ajax); every
    // collection is materialized BEFORE in-memory shaping (EF Core 10 safe).
    [Microsoft.AspNetCore.Mvc.Area("Property")]
    public class PropertyInsightsController : BaseController
    {
        ApplicationDbContext db;
        public PropertyInsightsController() { db = new ApplicationDbContext(); }

        static string J(object o) => System.Text.Json.JsonSerializer.Serialize(o);

        // ---------- shared snapshot of all property financials, materialized once ----------
        private List<PLRow> BuildPL()
        {
            var props = db.PropertyMains.Select(p => new { p.Id, p.Name, p.City, p.PropertyType }).ToList();
            var ptypes = db.PropertyTypes.Select(t => new { t.ID, t.Name }).ToList();
            var tc = db.TenancyContracts.Where(c => c.Status == 0).Select(c => new { c.Property, c.Rent }).ToList();
            var rentals = db.Rentals.Select(r => new { r.Property, r.Amount }).ToList();
            var maint = db.Maintenances.Select(m => new { m.Property, m.Amount }).ToList();
            var units = db.PropertyUnits.Select(u => new { u.Id, u.Property }).ToList();
            var occ = db.TenancyContracts.Where(c => c.Status == 0).Select(c => c.Unit).Distinct().ToList();

            return props.Select(p =>
            {
                decimal income = tc.Where(x => x.Property == p.Id).Sum(x => x.Rent ?? 0);
                decimal billed = rentals.Where(x => x.Property == p.Id).Sum(x => x.Amount);
                decimal expense = maint.Where(x => x.Property == p.Id).Sum(x => x.Amount);
                int tot = units.Count(u => u.Property == p.Id);
                int oc = units.Count(u => u.Property == p.Id && occ.Contains(u.Id));
                return new PLRow
                {
                    id = p.Id,
                    name = p.Name ?? "-",
                    city = p.City ?? "-",
                    ptype = ptypes.Where(t => t.ID == p.PropertyType).Select(t => t.Name).FirstOrDefault() ?? "-",
                    income = income,
                    billed = billed,
                    expense = expense,
                    net = income - expense,
                    units = tot,
                    occupied = oc,
                    vacant = tot - oc,
                    margin = income > 0 ? (int)Math.Round((income - expense) * 100m / income) : 0
                };
            }).OrderByDescending(x => x.net).ToList();
        }

        // ============================ PROPERTY P&L ============================
        [HttpGet]
        public ActionResult ProfitLoss()
        {
            var rows = BuildPL();
            ViewBag.TotIncome = rows.Sum(r => r.income);
            ViewBag.TotBilled = rows.Sum(r => r.billed);
            ViewBag.TotExpense = rows.Sum(r => r.expense);
            ViewBag.TotNet = rows.Sum(r => r.net);
            ViewBag.Profitable = rows.Count(r => r.net > 0);
            ViewBag.Loss = rows.Count(r => r.net < 0);
            ViewBag.OverallMargin = ViewBag.TotIncome > 0 ? (int)Math.Round(((decimal)ViewBag.TotNet) * 100m / (decimal)ViewBag.TotIncome) : 0;
            ViewBag.Rows = J(rows);
            // top properties by net for the chart
            ViewBag.TopChart = J(rows.Take(8).Select(r => new { label = r.name, income = r.income, expense = r.expense, net = r.net }).ToList());
            ViewBag.Active = "Reports";
            return View();
        }

        // ============================ CONTRACTOR ALLOCATION ============================
        [HttpGet]
        public ActionResult ContractorAllocation()
        {
            var maint = (from m in db.Maintenances
                         join c in db.Contractors on m.Contractor equals c.ContractorID into cc from c in cc.DefaultIfEmpty()
                         join p in db.PropertyMains on m.Property equals p.Id into pp from p in pp.DefaultIfEmpty()
                         select new { cid = (long?)(c == null ? 0 : c.ContractorID), cname = c.ContractorName, cphone = c.Location, pid = (long?)m.Property, pname = p.Name, amount = m.Amount, start = m.StartDate, end = m.EndDate, vno = m.VoucherNo }).ToList();

            var byContractor = maint.GroupBy(x => new { x.cid, x.cname }).Select(g => new
            {
                id = g.Key.cid ?? 0,
                name = g.Key.cname ?? "(unassigned)",
                contracts = g.Count(),
                properties = g.Select(x => x.pid).Distinct().Count(),
                expense = g.Sum(x => x.amount)
            }).OrderByDescending(x => x.expense).ToList();

            ViewBag.TotContractors = db.Contractors.Count();
            ViewBag.Allocated = byContractor.Count;
            ViewBag.Unallocated = db.Contractors.Count() - byContractor.Count(x => x.id != 0);
            ViewBag.TotContracts = maint.Count;
            ViewBag.TotExpense = maint.Sum(x => x.amount);
            ViewBag.ByContractor = J(byContractor);
            ViewBag.AllocChart = J(byContractor.Take(8).Select(x => new { label = x.name, value = x.expense }).ToList());
            ViewBag.Assignments = J(maint.OrderByDescending(x => x.amount).Take(40).Select(x => new { contractor = x.cname ?? "(unassigned)", property = x.pname ?? "-", amount = x.amount, voucher = x.vno ?? "-", start = x.start ?? "-", end = x.end ?? "-" }).ToList());
            ViewBag.Active = "Transactions";
            return View();
        }

        // ============================ UNIT INSIGHTS ============================
        [HttpGet]
        public ActionResult UnitInsights()
        {
            var props = db.PropertyMains.Select(p => new { p.Id, p.Name }).ToList();
            var utypes = db.PropertyUnitTypes.Select(t => new { t.ID, t.Name }).ToList();
            var units = db.PropertyUnits.Select(u => new { u.Id, u.Name, u.Code, u.Property, u.UnitType, u.Rent }).ToList();
            var occ = db.TenancyContracts.Where(c => c.Status == 0).Select(c => c.Unit).Distinct().ToList();

            ViewBag.TotUnits = units.Count;
            ViewBag.Occupied = units.Count(u => occ.Contains(u.Id));
            ViewBag.Vacant = units.Count(u => !occ.Contains(u.Id));
            ViewBag.PotentialRent = units.Sum(u => u.Rent ?? 0);
            ViewBag.RealisedRent = units.Where(u => occ.Contains(u.Id)).Sum(u => u.Rent ?? 0);
            ViewBag.LostRent = units.Where(u => !occ.Contains(u.Id)).Sum(u => u.Rent ?? 0);

            // occupancy by property
            var byProp = props.Select(p =>
            {
                var us = units.Where(u => u.Property == p.Id).ToList();
                int oc = us.Count(u => occ.Contains(u.Id));
                return new { property = p.Name ?? "-", units = us.Count, occupied = oc, vacant = us.Count - oc, pct = us.Count > 0 ? (int)Math.Round(oc * 100.0 / us.Count) : 0, rent = us.Sum(u => u.Rent ?? 0) };
            }).Where(x => x.units > 0).OrderByDescending(x => x.units).ToList();
            ViewBag.ByProperty = J(byProp);

            // by unit type
            var byType = units.GroupBy(u => utypes.Where(t => t.ID == u.UnitType).Select(t => t.Name).FirstOrDefault() ?? "Other")
                .Select(g => new { type = g.Key, count = g.Count(), occupied = g.Count(u => occ.Contains(u.Id)), rent = g.Sum(u => u.Rent ?? 0) })
                .OrderByDescending(x => x.count).ToList();
            ViewBag.ByType = J(byType);

            // vacant units list
            ViewBag.VacantList = J(units.Where(u => !occ.Contains(u.Id))
                .Select(u => new { unit = u.Name ?? "-", code = u.Code ?? "-", property = props.Where(p => p.Id == u.Property).Select(p => p.Name).FirstOrDefault() ?? "-", type = utypes.Where(t => t.ID == u.UnitType).Select(t => t.Name).FirstOrDefault() ?? "-", rent = u.Rent ?? 0 })
                .OrderByDescending(x => x.rent).Take(40).ToList());
            ViewBag.Active = "Master";
            return View();
        }

        // ============================ PORTFOLIO PERFORMANCE (ROI / YIELD) ============================
        [HttpGet]
        public ActionResult Performance()
        {
            var pl = BuildPL();
            var regs = db.PropertyRegistrations.Select(r => new { r.Property, r.Amount }).ToList();
            var rows = pl.Select(p =>
            {
                decimal value = regs.Where(x => x.Property == p.id).Sum(x => x.Amount);
                decimal yld = value > 0 ? Math.Round(p.net * 100m / value, 1) : 0;
                decimal payback = (p.net > 0 && value > 0) ? Math.Round(value / p.net, 1) : 0;
                return new { id = p.id, name = p.name, city = p.city, ptype = p.ptype, income = p.income, expense = p.expense, net = p.net, value, yld, payback, units = p.units, occupied = p.occupied };
            }).OrderByDescending(x => x.yld).ToList();
            decimal totValue = rows.Sum(r => r.value);
            decimal totNet = rows.Sum(r => r.net);
            ViewBag.TotValue = totValue;
            ViewBag.TotNet = totNet;
            ViewBag.AvgYield = totValue > 0 ? Math.Round(totNet * 100m / totValue, 1) : 0;
            ViewBag.Valued = rows.Count(r => r.value > 0);
            ViewBag.BestYield = rows.Where(r => r.value > 0).Select(r => (decimal?)r.yld).Max() ?? 0;
            ViewBag.Rows = J(rows);
            ViewBag.Chart = J(rows.Where(r => r.value > 0).Take(8).Select(r => new { label = r.name, value = r.yld }).ToList());
            ViewBag.Active = "Reports";
            return View();
        }

        // ============================ GEOGRAPHIC / LOCATION DISTRIBUTION ============================
        [HttpGet]
        public ActionResult Locations()
        {
            var props = db.PropertyMains.Select(p => new { p.Id, p.City, p.State }).ToList();
            var units = db.PropertyUnits.Select(u => new { u.Id, u.Property, u.Rent }).ToList();
            var occ = db.TenancyContracts.Where(c => c.Status == 0).Select(c => c.Unit).Distinct().ToList();
            var maint = db.Maintenances.Select(m => new { m.Property, m.Amount }).ToList();

            LocRow Make(string key, List<long> pids)
            {
                var us = units.Where(u => u.Property.HasValue && pids.Contains(u.Property.Value)).ToList();
                int oc = us.Count(u => occ.Contains(u.Id));
                return new LocRow
                {
                    location = key,
                    properties = pids.Count,
                    units = us.Count,
                    occupied = oc,
                    vacant = us.Count - oc,
                    pct = us.Count > 0 ? (int)Math.Round(oc * 100.0 / us.Count) : 0,
                    rent = us.Sum(u => u.Rent ?? 0),
                    expense = maint.Where(m => pids.Contains(m.Property)).Sum(m => m.Amount)
                };
            }

            var byCity = props.GroupBy(p => string.IsNullOrEmpty(p.City) ? "Unspecified" : p.City)
                              .Select(g => Make(g.Key, g.Select(x => x.Id).ToList())).OrderByDescending(x => x.properties).ToList();
            var byState = props.GroupBy(p => string.IsNullOrEmpty(p.State) ? "Unspecified" : p.State)
                               .Select(g => Make(g.Key, g.Select(x => x.Id).ToList())).OrderByDescending(x => x.properties).ToList();

            ViewBag.ByCity = J(byCity);
            ViewBag.ByState = J(byState);
            ViewBag.Cities = props.Select(p => p.City).Where(c => !string.IsNullOrEmpty(c)).Distinct().Count();
            ViewBag.States = props.Select(p => p.State).Where(c => !string.IsNullOrEmpty(c)).Distinct().Count();
            ViewBag.TotProps = props.Count;
            ViewBag.TotUnits = units.Count;
            ViewBag.Active = "Master";
            return View();
        }

        // ============================ EXPIRY & RENEWALS ACTION CENTRE ============================
        [HttpGet]
        public ActionResult Renewals()
        {
            var today = DateTime.Now;
            var contracts = (from c in db.TenancyContracts
                             where c.Status == 0 && c.EndDate >= today
                             join t in db.Tenants on c.Tenant equals t.TenantID into tt from t in tt.DefaultIfEmpty()
                             join p in db.PropertyMains on c.Property equals p.Id into pp from p in pp.DefaultIfEmpty()
                             join u in db.PropertyUnits on c.Unit equals u.Id into uu from u in uu.DefaultIfEmpty()
                             select new { id = c.Id, party = t.TenantName, property = p.Name, unit = u.Name, end = c.EndDate, amount = c.Rent }).ToList();
            var docs = (from d in db.PropertyDocumentTypes
                        where d.ExpDate >= today
                        join p in db.PropertyMains on d.Reference equals p.Id into pp from p in pp.DefaultIfEmpty()
                        join dt in db.DocumentTypes on d.DocumentType equals dt.ID into dd from dt in dd.DefaultIfEmpty()
                        select new { id = d.ID, name = dt.Name, property = p.Name, exp = d.ExpDate }).ToList();

            var items = new List<RenewItem>();
            foreach (var c in contracts)
            {
                int dleft = (int)(c.end - today).TotalDays;
                items.Add(new RenewItem { type = "Tenancy Contract", title = c.party ?? "Tenant", sub = (c.property ?? "-") + " / " + (c.unit ?? "-"), date = c.end.ToString("dd-MM-yyyy"), days = dleft, amount = c.amount ?? 0, link = "/Property/TenancyContract/Edit/" + c.id, tier = dleft <= 30 ? 0 : (dleft <= 60 ? 1 : 2) });
            }
            foreach (var d in docs)
            {
                var ex = d.exp ?? today; int dleft = (int)(ex - today).TotalDays;
                items.Add(new RenewItem { type = "Document", title = d.name ?? "Document", sub = d.property ?? "-", date = ex.ToString("dd-MM-yyyy"), days = dleft, amount = 0m, link = "/Property/PropertyHome/Dashboard", tier = dleft <= 30 ? 0 : (dleft <= 60 ? 1 : 2) });
            }
            var ordered = items.OrderBy(x => x.days).ToList();
            ViewBag.Critical = ordered.Count(x => x.tier == 0);
            ViewBag.Soon = ordered.Count(x => x.tier == 1);
            ViewBag.Later = ordered.Count(x => x.tier == 2);
            ViewBag.Total = ordered.Count;
            ViewBag.Items = J(ordered);
            ViewBag.Active = "Reports";
            return View();
        }

        // ============================ FINANCIAL TREND (month-wise) ============================
        [HttpGet]
        public ActionResult FinancialTrend()
        {
            var today = DateTime.Now;
            var monthStart = new DateTime(today.Year, today.Month, 1);
            var from = monthStart.AddMonths(-7);
            var rentals = db.Rentals.Where(r => r.RDate >= from).Select(r => new { r.RDate, r.Amount }).ToList();
            var maint = db.Maintenances.Where(m => m.Date >= from).Select(m => new { m.Date, m.Amount }).ToList();

            var months = new List<object>();
            decimal totInc = 0, totExp = 0;
            for (int i = 7; i >= 0; i--)
            {
                var s = monthStart.AddMonths(-i); var e = s.AddMonths(1);
                decimal inc = rentals.Where(r => r.RDate >= s && r.RDate < e).Sum(r => r.Amount);
                decimal exp = maint.Where(m => m.Date >= s && m.Date < e).Sum(m => m.Amount);
                totInc += inc; totExp += exp;
                months.Add(new { label = s.ToString("MMM yy"), income = inc, expense = exp, net = inc - exp });
            }
            ViewBag.Months = J(months);
            ViewBag.TotIncome = totInc;
            ViewBag.TotExpense = totExp;
            ViewBag.TotNet = totInc - totExp;
            ViewBag.AvgIncome = Math.Round(totInc / 8m, 0);
            ViewBag.Active = "Reports";
            return View();
        }

        // ============================ TENANT LEDGER / STATEMENT ============================
        [HttpGet]
        public ActionResult TenantLedger(long? id)
        {
            var tenants = db.Tenants.Select(t => new { t.TenantID, t.TenantName }).ToList();
            ViewBag.Tenants = J(tenants.Select(t => new { id = t.TenantID, name = t.TenantName ?? "-" }).ToList());
            if (id == null || id == 0) id = tenants.Select(t => (long?)t.TenantID).FirstOrDefault();
            ViewBag.SelectedId = id ?? 0;

            if (id != null && id != 0)
            {
                var t = db.Tenants.Where(x => x.TenantID == id).Select(x => new { x.TenantName, x.TenantCode, x.CreditLimit, x.Location }).FirstOrDefault();
                ViewBag.TenantName = t?.TenantName ?? "-";
                ViewBag.TenantCode = t?.TenantCode ?? "-";
                ViewBag.CreditLimit = t?.CreditLimit ?? 0;

                var contracts = (from c in db.TenancyContracts
                                 where c.Tenant == id
                                 join p in db.PropertyMains on c.Property equals p.Id into pp from p in pp.DefaultIfEmpty()
                                 join u in db.PropertyUnits on c.Unit equals u.Id into uu from u in uu.DefaultIfEmpty()
                                 select new { c.Id, property = p.Name, unit = u.Name, c.StartDate, c.EndDate, c.Rent, c.Deposit, c.Status }).ToList();
                var rentals = (from r in db.Rentals
                               where r.Tenant == id
                               join p in db.PropertyMains on r.Property equals p.Id into pp from p in pp.DefaultIfEmpty()
                               orderby r.RDate descending
                               select new { r.RentalID, property = p.Name, r.RDate, r.Amount, r.VoucherNo }).ToList();

                ViewBag.Contracts = J(contracts.Select(c => new { c.Id, property = c.property ?? "-", unit = c.unit ?? "-", start = c.StartDate.ToString("dd-MM-yyyy"), end = c.EndDate.ToString("dd-MM-yyyy"), rent = c.Rent ?? 0, deposit = c.Deposit ?? 0, active = c.Status == 0 }).ToList());
                ViewBag.Rentals = J(rentals.Take(30).Select(r => new { r.RentalID, property = r.property ?? "-", date = r.RDate.ToString("dd-MM-yyyy"), amount = r.Amount, voucher = r.VoucherNo ?? "-" }).ToList());
                ViewBag.ActiveContracts = contracts.Count(c => c.Status == 0);
                ViewBag.TotContracted = contracts.Where(c => c.Status == 0).Sum(c => c.Rent ?? 0);
                ViewBag.TotBilled = rentals.Sum(r => r.Amount);
                ViewBag.TotDeposit = contracts.Sum(c => c.Deposit ?? 0);
            }
            ViewBag.Active = "Master";
            return View();
        }

        // ============================ LANDLORD PAYOUT ============================
        [HttpGet]
        public ActionResult LandlordPayout()
        {
            const decimal commPct = 5m; // management commission %
            var landlords = db.Landlords.Select(l => new { l.LandlordID, l.LandlordName }).ToList();
            var props = db.PropertyMains.Select(p => new { p.Id, p.LandlordID }).ToList();
            var tc = db.TenancyContracts.Where(c => c.Status == 0).Select(c => new { c.Property, c.Rent }).ToList();
            // collected = actual money received (rent receipts), per owner decision — so commission/payout never exceed real collections
            var receipts = db.PropertyRentReceipts.Where(r => r.Status == Status.active).Select(r => new { r.Property, r.Amount }).ToList();
            var maint = db.Maintenances.Select(m => new { m.Property, m.Amount }).ToList();

            var rows = landlords.Select(l =>
            {
                var pids = props.Where(p => (p.LandlordID ?? 0) == l.LandlordID).Select(p => p.Id).ToList();
                decimal income = tc.Where(x => pids.Contains(x.Property ?? 0)).Sum(x => x.Rent ?? 0);
                decimal collected = receipts.Where(x => pids.Contains(x.Property)).Sum(x => x.Amount);
                decimal expense = maint.Where(x => pids.Contains(x.Property)).Sum(x => x.Amount);
                decimal commission = Math.Round(collected * commPct / 100m, 2);
                decimal payout = collected - commission - expense;
                return new { id = l.LandlordID, name = l.LandlordName ?? "-", properties = pids.Count, income, collected, expense, commission, payout };
            }).Where(x => x.properties > 0).OrderByDescending(x => x.payout).ToList();

            ViewBag.CommPct = commPct;
            ViewBag.TotCollected = rows.Sum(r => r.collected);
            ViewBag.TotCommission = rows.Sum(r => r.commission);
            ViewBag.TotExpense = rows.Sum(r => r.expense);
            ViewBag.TotPayout = rows.Sum(r => r.payout);
            ViewBag.Landlords = rows.Count;
            ViewBag.Rows = J(rows);
            ViewBag.Active = "Reports";
            return View();
        }

        // ============================ PROPERTY COMPARISON ============================
        [HttpGet]
        public ActionResult Comparison()
        {
            var pl = BuildPL();
            var regs = db.PropertyRegistrations.Select(r => new { r.Property, r.Amount }).ToList();
            var rows = pl.Select(p =>
            {
                decimal value = regs.Where(x => x.Property == p.id).Sum(x => x.Amount);
                decimal yld = value > 0 ? Math.Round(p.net * 100m / value, 1) : 0;
                int occPct = p.units > 0 ? (int)Math.Round(p.occupied * 100.0 / p.units) : 0;
                return new { id = p.id, name = p.name, ptype = p.ptype, city = p.city, units = p.units, occupied = p.occupied, vacant = p.vacant, occPct, income = p.income, expense = p.expense, net = p.net, value, yld };
            }).ToList();
            ViewBag.Rows = J(rows);
            ViewBag.Active = "Reports";
            return View();
        }

        // ============================ RENT COLLECTION ============================
        [HttpGet]
        public ActionResult RentCollection()
        {
            var rentals = (from r in db.Rentals
                           where r.Status == Status.active
                           join t in db.Tenants on r.Tenant equals t.TenantID into tt from t in tt.DefaultIfEmpty()
                           join p in db.PropertyMains on r.Property equals p.Id into pp from p in pp.DefaultIfEmpty()
                           select new { r.RentalID, tenant = t.TenantName, property = p.Name, r.RDate, r.Amount, r.VoucherNo }).ToList();

            // RECEIPT-BASED: a rent invoice is collected when an active receipt exists for it.
            var receipts = db.PropertyRentReceipts.Where(x => x.Status == Status.active)
                             .Select(x => new { x.RentalID, x.Amount, x.ReceiptDate, x.ReceiptNo, x.Mode }).ToList();
            var rentalIds = new HashSet<long>(rentals.Select(r => r.RentalID));
            var paidRentalIds = new HashSet<long>(receipts.Select(x => x.RentalID));
            var receiptByRental = receipts.GroupBy(x => x.RentalID).ToDictionary(g => g.Key, g => g.Sum(x => x.Amount));

            decimal billed = rentals.Sum(r => r.Amount);
            decimal collected = receipts.Where(x => rentalIds.Contains(x.RentalID)).Sum(x => x.Amount);
            decimal outstanding = billed - collected;

            var byTenant = rentals.GroupBy(r => r.tenant ?? "-").Select(g =>
            {
                decimal b = g.Sum(x => x.Amount);
                decimal c = g.Sum(x => receiptByRental.TryGetValue(x.RentalID, out var amt) ? amt : 0); // collected = actual receipt amounts, to reconcile with the headline total
                return new { tenant = g.Key, billed = b, collected = c, outstanding = b - c, pct = b > 0 ? (int)Math.Round(c * 100m / b, 0) : 0 };
            }).OrderByDescending(x => x.outstanding).ToList();

            // outstanding rent invoices (no receipt yet) — each can be collected inline
            var pending = rentals.Where(r => !paidRentalIds.Contains(r.RentalID)).OrderBy(r => r.RDate)
                .Select(r => new { rentalId = r.RentalID, tenant = r.tenant ?? "-", property = r.property ?? "-", voucher = r.VoucherNo ?? "-", date = r.RDate.ToString("dd-MM-yyyy"), amount = r.Amount }).ToList();

            // recent receipts ledger
            var recent = (from x in receipts
                          join r in rentals on x.RentalID equals r.RentalID into rr from r in rr.DefaultIfEmpty()
                          orderby x.ReceiptDate descending
                          select new { x.ReceiptNo, tenant = r == null ? "-" : (r.tenant ?? "-"), x.Amount, x.ReceiptDate, x.Mode }).Take(40).ToList();

            ViewBag.Billed = billed;
            ViewBag.Collected = collected;
            ViewBag.Outstanding = outstanding;
            ViewBag.CollectedPct = billed > 0 ? (int)Math.Round(collected * 100m / billed, 0) : 0;
            ViewBag.PendingCount = pending.Count;
            ViewBag.ReceiptCount = receipts.Count;
            ViewBag.ByTenant = J(byTenant);
            ViewBag.Pending = J(pending);
            ViewBag.Recent = J(recent.Select(x => new { receiptNo = x.ReceiptNo ?? "-", tenant = x.tenant, amount = x.Amount, date = x.ReceiptDate.ToString("dd-MM-yyyy"), mode = x.Mode ?? "-" }).ToList());
            ViewBag.Active = "Transactions";
            return View();
        }

        // record an actual rent receipt against an outstanding invoice (receipt-based collection)
        [HttpPost]
        public ActionResult RecordReceipt(long rentalId, string mode)
        {
            var r = db.Rentals.Where(x => x.RentalID == rentalId)
                      .Select(x => new { x.RentalID, x.Tenant, x.Property, x.Unit, x.Amount }).FirstOrDefault();
            if (r != null && !db.PropertyRentReceipts.Any(x => x.RentalID == rentalId && x.Status == Status.active))
            {
                long n = db.PropertyRentReceipts.Select(x => (long?)x.ID).Max() ?? 0;
                db.PropertyRentReceipts.Add(new PropertyRentReceipt
                {
                    ReceiptNo = "RC-" + (n + 1).ToString("00000"),
                    RentalID = r.RentalID,
                    Tenant = r.Tenant,
                    Property = r.Property,
                    Unit = r.Unit,
                    Amount = r.Amount,
                    ReceiptDate = DateTime.Now,
                    Mode = string.IsNullOrEmpty(mode) ? "Cash" : mode,
                    ChequeNo = null,
                    Note = "Recorded from Rent Collection",
                    CreatedDate = DateTime.Now,
                    CreatedBy = User.Identity.GetUserId(),
                    Status = Status.active
                });
                db.SaveChanges();
            }
            return RedirectToAction("RentCollection");
        }

        // ============================ AMC / MAINTENANCE CALENDAR ============================
        [HttpGet]
        public ActionResult MaintenanceCalendar(int? y, int? m)
        {
            var today = DateTime.Today;
            int year = (y == null || y < 2000 || y > 2100) ? today.Year : y.Value;
            int month = (m == null || m < 1 || m > 12) ? today.Month : m.Value;
            var monthStart = new DateTime(year, month, 1);
            var monthEnd = monthStart.AddMonths(1);

            var propNames = db.PropertyMains.Select(p => new { p.Id, p.Name }).ToList()
                              .ToDictionary(p => p.Id, p => p.Name ?? "-");
            var contrNames = db.Contractors.Select(c => new { c.ContractorID, c.ContractorName }).ToList()
                               .ToDictionary(c => c.ContractorID, c => c.ContractorName ?? "-");
            Func<long, string> pn = id => propNames.ContainsKey(id) ? propNames[id] : "-";
            Func<long, string> cn = id => contrNames.ContainsKey(id) ? contrNames[id] : "-";

            var events = new List<object>();

            // 1. maintenance / AMC events on their date
            var maint = db.Maintenances.Where(x => x.Status == Status.active && x.Date >= monthStart && x.Date < monthEnd)
                          .Select(x => new { x.Date, x.Property, x.Contractor, x.Amount, x.ContractType, x.VoucherNo }).ToList();
            foreach (var x in maint)
            {
                bool amc = x.ContractType != null && x.ContractType > 0;
                events.Add(new
                {
                    date = x.Date.ToString("yyyy-MM-dd"),
                    day = x.Date.Day,
                    kind = amc ? "amc" : "maint",
                    title = (amc ? "AMC · " : "Maintenance · ") + pn(x.Property),
                    sub = cn(x.Contractor) + " · " + x.Amount.ToString("#,##0"),
                    amount = x.Amount
                });
            }

            // 2. tenancy renewals due (contract expiry within the month)
            var renew = db.TenancyContracts.Where(c => c.Status == Status.active && c.EndDate >= monthStart && c.EndDate < monthEnd)
                          .Select(c => new { c.EndDate, c.Property, c.Rent }).ToList();
            foreach (var c in renew)
            {
                long pid = c.Property ?? 0;
                events.Add(new
                {
                    date = c.EndDate.ToString("yyyy-MM-dd"),
                    day = c.EndDate.Day,
                    kind = "renew",
                    title = "Renewal · " + pn(pid),
                    sub = "Contract expiry · " + (c.Rent ?? 0).ToString("#,##0"),
                    amount = c.Rent ?? 0
                });
            }

            // 3. inspection / maintenance tasks scheduled this month (open tasks only)
            var taskEv = db.PropertyMaintenanceTasks.Where(t => t.Status != 2 && t.ScheduledDate >= monthStart && t.ScheduledDate < monthEnd)
                          .Select(t => new { t.Title, t.ScheduledDate, t.Property, t.TaskType }).ToList();
            foreach (var t in taskEv)
            {
                events.Add(new
                {
                    date = t.ScheduledDate.ToString("yyyy-MM-dd"),
                    day = t.ScheduledDate.Day,
                    kind = "task",
                    title = "Task · " + (t.Title ?? "-"),
                    sub = (t.TaskType ?? "Task") + " · " + pn(t.Property),
                    amount = 0m
                });
            }

            // upcoming 60-day agenda (side panel)
            var upTo = today.AddDays(60);
            var upMaint = db.Maintenances.Where(x => x.Status == Status.active && x.Date >= today && x.Date <= upTo)
                            .Select(x => new { x.Date, x.Property, x.Contractor, x.Amount }).ToList()
                            .Select(x => new { date = x.Date, kind = "maint", title = pn(x.Property), sub = cn(x.Contractor), amount = x.Amount });
            var upRenew = db.TenancyContracts.Where(c => c.Status == Status.active && c.EndDate >= today && c.EndDate <= upTo)
                            .Select(c => new { c.EndDate, c.Property, c.Rent }).ToList()
                            .Select(c => new { date = c.EndDate, kind = "renew", title = pn(c.Property ?? 0), sub = "Contract expiry", amount = c.Rent ?? 0 });
            var upTask = db.PropertyMaintenanceTasks.Where(t => t.Status != 2 && t.ScheduledDate >= today && t.ScheduledDate <= upTo)
                            .Select(t => new { t.ScheduledDate, t.Title, t.TaskType, t.Property }).ToList()
                            .Select(t => new { date = t.ScheduledDate, kind = "task", title = (t.Title ?? "Task"), sub = (t.TaskType ?? ""), amount = 0m });
            var upcoming = upMaint.Concat(upRenew).Concat(upTask).OrderBy(x => x.date)
                            .Select(x => new { date = x.date.ToString("dd MMM"), x.kind, x.title, x.sub, x.amount }).ToList();

            int pmo = month == 1 ? 12 : month - 1; int pyr = month == 1 ? year - 1 : year;
            int nmo = month == 12 ? 1 : month + 1; int nyr = month == 12 ? year + 1 : year;
            ViewBag.Year = year; ViewBag.Month = month;
            ViewBag.PrevY = pyr; ViewBag.PrevM = pmo; ViewBag.NextY = nyr; ViewBag.NextM = nmo;
            ViewBag.MonthName = monthStart.ToString("MMMM yyyy");
            ViewBag.FirstDow = (int)monthStart.DayOfWeek;     // 0 = Sunday
            ViewBag.DaysInMonth = DateTime.DaysInMonth(year, month);
            ViewBag.TodayDay = (today.Year == year && today.Month == month) ? today.Day : 0;
            ViewBag.Events = J(events);
            ViewBag.Upcoming = J(upcoming);
            ViewBag.MaintCount = maint.Count;
            ViewBag.RenewCount = renew.Count;
            ViewBag.AmcCount = maint.Count(x => x.ContractType != null && x.ContractType > 0);
            ViewBag.MonthExpense = maint.Sum(x => x.Amount);
            ViewBag.Active = "Transactions";
            return View();
        }

        // ============================ INSPECTION / MAINTENANCE TASK SCHEDULER ============================
        [HttpGet]
        public ActionResult InspectionScheduler()
        {
            var today = DateTime.Today;
            var props = db.PropertyMains.Select(p => new { p.Id, p.Name }).ToList();
            var contractors = db.Contractors.Select(c => new { c.ContractorID, c.ContractorName }).ToList();
            var propMap = props.ToDictionary(p => p.Id, p => p.Name ?? "-");
            var contrMap = contractors.ToDictionary(c => c.ContractorID, c => c.ContractorName ?? "-");
            Func<long, string> pn = id => propMap.ContainsKey(id) ? propMap[id] : "-";
            Func<long, string> cn = id => (id != 0 && contrMap.ContainsKey(id)) ? contrMap[id] : "(unassigned)";

            var tasks = db.PropertyMaintenanceTasks
                .Select(t => new { t.ID, t.Title, t.TaskType, t.Property, t.Contractor, t.ScheduledDate, t.Priority, t.Status, t.Notes }).ToList();

            var rows = tasks
                .OrderBy(t => t.Status == 2).ThenBy(t => t.ScheduledDate)
                .Select(t => new
                {
                    id = t.ID,
                    title = t.Title ?? "-",
                    type = t.TaskType ?? "-",
                    property = pn(t.Property),
                    contractor = cn(t.Contractor),
                    date = t.ScheduledDate.ToString("dd MMM yyyy"),
                    priority = t.Priority ?? "Normal",
                    status = t.Status,
                    statusName = t.Status == 0 ? "Pending" : t.Status == 1 ? "In Progress" : "Completed",
                    overdue = t.Status != 2 && t.ScheduledDate.Date < today,
                    notes = t.Notes ?? ""
                }).ToList();

            ViewBag.Total = tasks.Count;
            ViewBag.Pending = tasks.Count(t => t.Status == 0);
            ViewBag.InProgress = tasks.Count(t => t.Status == 1);
            ViewBag.Done = tasks.Count(t => t.Status == 2);
            ViewBag.Overdue = tasks.Count(t => t.Status != 2 && t.ScheduledDate.Date < today);
            ViewBag.Tasks = J(rows);
            ViewBag.Properties = J(props.Select(p => new { id = p.Id, name = p.Name ?? "-" }).ToList());
            ViewBag.Contractors = J(contractors.Select(c => new { id = c.ContractorID, name = c.ContractorName ?? "-" }).ToList());
            ViewBag.Active = "Transactions";
            return View();
        }

        [HttpPost]
        public ActionResult CreateTask(string title, string taskType, long property, long contractor, string scheduledDate, string priority, string notes)
        {
            if (!string.IsNullOrWhiteSpace(title))
            {
                DateTime sd;
                if (string.IsNullOrWhiteSpace(scheduledDate) ||
                    !DateTime.TryParse(scheduledDate, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out sd))
                    sd = DateTime.Now;
                db.PropertyMaintenanceTasks.Add(new PropertyMaintenanceTask
                {
                    Title = title,
                    TaskType = string.IsNullOrWhiteSpace(taskType) ? "Maintenance" : taskType,
                    Property = property,
                    Unit = 0,
                    Contractor = contractor,
                    ScheduledDate = sd,
                    Priority = string.IsNullOrWhiteSpace(priority) ? "Normal" : priority,
                    Status = 0,
                    Notes = notes,
                    CreatedDate = DateTime.Now,
                    CreatedBy = User.Identity.GetUserId()
                });
                db.SaveChanges();
            }
            return RedirectToAction("InspectionScheduler");
        }

        [HttpPost]
        public ActionResult UpdateTaskStatus(long id, int status)
        {
            var t = db.PropertyMaintenanceTasks.FirstOrDefault(x => x.ID == id);
            if (t != null)
            {
                t.Status = status;
                t.CompletedDate = status == 2 ? (DateTime?)DateTime.Now : null;
                db.SaveChanges();
            }
            return RedirectToAction("InspectionScheduler");
        }

        // ============================ EXPIRY REMINDERS (email) ============================
        [HttpGet]
        public ActionResult Reminders()
        {
            var autoRow = db.EnableSettings.FirstOrDefault(e => e.EnableType == "ReminderAutoSend");
            bool autoOn = autoRow != null && autoRow.Status == Status.active;
            var daysRow = db.EnableSettings.FirstOrDefault(e => e.EnableType == "ReminderDaysAhead");
            int days = 30; if (daysRow != null) int.TryParse(daysRow.TypeValue, out days); if (days <= 0) days = 30;

            var due = QuickSoft.Helpers.PropertyReminders.Upcoming(db, days);
            ViewBag.AutoOn = autoOn;
            ViewBag.Days = days;
            ViewBag.DueCount = due.Count;
            ViewBag.WithEmail = due.Count(d => !string.IsNullOrWhiteSpace(d.Email) && d.Email.Contains("@"));
            ViewBag.AlreadySent = due.Count(d => d.AlreadySent);
            ViewBag.Due = J(due.Select(d => new { d.Code, d.Tenant, d.Property, email = d.Email ?? "", exp = d.Expiry.ToString("dd-MM-yyyy"), d.Days, rent = d.Rent, sent = d.AlreadySent }).ToList());

            var log = db.PropertyReminderLogs.OrderByDescending(l => l.SentDate).Take(50).ToList();
            ViewBag.Log = J(log.Select(l => new { title = l.Title ?? "-", email = l.ToEmail ?? "-", exp = (l.ExpiryDate ?? DateTime.Now).ToString("dd-MM-yyyy"), sent = l.SentDate.ToString("dd-MM-yyyy HH:mm"), result = l.Result ?? "-" }).ToList());
            ViewBag.SentTotal = db.PropertyReminderLogs.Count(l => l.Result == "Sent");
            ViewBag.Msg = TempData["ReminderMsg"] as string ?? "";
            ViewBag.Active = "Settings";
            return View();
        }

        [HttpPost]
        public ActionResult SendReminders()
        {
            var r = QuickSoft.Helpers.PropertyReminders.Run(auto: false);
            TempData["ReminderMsg"] = "Processed " + r.Considered + ": " + r.Sent + " sent, " + r.NoEmail + " without email, " + r.Failed + " failed, " + r.Skipped + " already sent.";
            return RedirectToAction("Reminders");
        }

        [HttpPost]
        public ActionResult ToggleReminderAuto()
        {
            var row = db.EnableSettings.FirstOrDefault(e => e.EnableType == "ReminderAutoSend");
            if (row == null)
            {
                db.EnableSettings.Add(new EnableSetting { EnableType = "ReminderAutoSend", Status = Status.active, TypeValue = "0" });
            }
            else
            {
                row.Status = row.Status == Status.active ? Status.inactive : Status.active;
            }
            db.SaveChanges();
            TempData["ReminderMsg"] = "Auto-send setting updated.";
            return RedirectToAction("Reminders");
        }

        // ============================ PROPERTY 360 (single-property hub) ============================
        [HttpGet]
        public ActionResult Property360(long? id)
        {
            var allProps = db.PropertyMains.Select(p => new { p.Id, p.Name }).ToList();
            ViewBag.Properties = J(allProps.Select(p => new { id = p.Id, name = p.Name ?? "-" }).ToList());
            if (id == null || id == 0) id = allProps.Select(p => (long?)p.Id).FirstOrDefault();
            ViewBag.SelectedId = id ?? 0;

            if (id != null && id != 0)
            {
                var p = db.PropertyMains.Where(x => x.Id == id).Select(x => new { x.Name, x.Code, x.City, x.State, x.Address, x.PropertyType, x.LandlordID }).FirstOrDefault();
                ViewBag.PName = p?.Name ?? "-";
                ViewBag.PCode = p?.Code ?? "-";
                ViewBag.PCity = (p?.City ?? "") + (string.IsNullOrEmpty(p?.State) ? "" : ", " + p.State);
                ViewBag.PAddress = p?.Address ?? "-";
                long ptype = p?.PropertyType ?? 0; long pll = p?.LandlordID ?? 0;
                ViewBag.PType = db.PropertyTypes.Where(t => t.ID == ptype).Select(t => t.Name).FirstOrDefault() ?? "-";
                ViewBag.PLandlord = db.Landlords.Where(l => l.LandlordID == pll).Select(l => l.LandlordName).FirstOrDefault() ?? "—";

                var occ = db.TenancyContracts.Where(c => c.Status == 0).Select(c => c.Unit).Distinct().ToList();
                var units = (from u in db.PropertyUnits where u.Property == id
                             join t in db.PropertyUnitTypes on u.UnitType equals t.ID into tt from t in tt.DefaultIfEmpty()
                             select new { u.Id, u.Name, u.Code, type = t.Name, u.Rent }).ToList();
                ViewBag.Units = J(units.Select(u => new { name = u.Name ?? "-", code = u.Code ?? "-", type = u.type ?? "-", rent = u.Rent ?? 0, occupied = occ.Contains(u.Id) }).ToList());

                var contracts = (from c in db.TenancyContracts where c.Property == id
                                 join tn in db.Tenants on c.Tenant equals tn.TenantID into tt from tn in tt.DefaultIfEmpty()
                                 join u in db.PropertyUnits on c.Unit equals u.Id into uu from u in uu.DefaultIfEmpty()
                                 select new { c.Id, tenant = tn.TenantName, unit = u.Name, c.StartDate, c.EndDate, c.Rent, c.Status }).ToList();
                ViewBag.Contracts = J(contracts.Select(c => new { c.Id, tenant = c.tenant ?? "-", unit = c.unit ?? "-", start = c.StartDate.ToString("dd-MM-yyyy"), end = c.EndDate.ToString("dd-MM-yyyy"), rent = c.Rent ?? 0, active = c.Status == 0 }).ToList());

                var maint = (from mn in db.Maintenances where mn.Property == id
                             join ct in db.Contractors on mn.Contractor equals ct.ContractorID into cc from ct in cc.DefaultIfEmpty()
                             orderby mn.Date descending
                             select new { contractor = ct.ContractorName, mn.Amount, mn.Date, mn.VoucherNo }).ToList();
                ViewBag.Maint = J(maint.Select(mn => new { contractor = mn.contractor ?? "-", amount = mn.Amount, date = mn.Date.ToString("dd-MM-yyyy"), voucher = mn.VoucherNo ?? "-" }).ToList());

                var docs = (from d in db.PropertyDocumentTypes where d.Reference == id
                            join dt in db.DocumentTypes on d.DocumentType equals dt.ID into dd from dt in dd.DefaultIfEmpty()
                            select new { name = dt.Name, d.ExpDate, d.Purpose }).ToList();
                ViewBag.Docs = J(docs.Select(d => new { name = d.name ?? "Document", exp = (d.ExpDate ?? DateTime.Now).ToString("dd-MM-yyyy"), purpose = d.Purpose ?? "-" }).ToList());

                int tot = units.Count; int oc = units.Count(u => occ.Contains(u.Id));
                ViewBag.TotUnits = tot; ViewBag.Occupied = oc; ViewBag.Vacant = tot - oc;
                decimal inc = contracts.Where(c => c.Status == 0).Sum(c => c.Rent ?? 0);
                decimal exp = maint.Sum(mn => mn.Amount);
                ViewBag.Income = inc; ViewBag.Expense = exp; ViewBag.Net = inc - exp;
            }
            ViewBag.Active = "Master";
            return View();
        }

        // ============================ UNIT AVAILABILITY BOARD ============================
        [HttpGet]
        public ActionResult Availability()
        {
            var props = db.PropertyMains.Select(p => new { p.Id, p.Name }).ToList();
            var units = (from u in db.PropertyUnits
                         join t in db.PropertyUnitTypes on u.UnitType equals t.ID into tt from t in tt.DefaultIfEmpty()
                         select new { u.Id, u.Name, u.Property, type = t.Name, u.Rent }).ToList();
            var occ = db.TenancyContracts.Where(c => c.Status == 0).Select(c => c.Unit).Distinct().ToList();

            var board = props.Select(p => new
            {
                property = p.Name ?? "-",
                units = units.Where(u => u.Property == p.Id)
                             .Select(u => new { name = u.Name ?? "-", type = u.type ?? "-", rent = u.Rent ?? 0, occupied = occ.Contains(u.Id) }).ToList()
            }).Where(x => x.units.Any()).ToList();

            ViewBag.Board = J(board);
            ViewBag.TotUnits = units.Count;
            ViewBag.Occupied = units.Count(u => occ.Contains(u.Id));
            ViewBag.Vacant = units.Count(u => !occ.Contains(u.Id));
            ViewBag.VacantValue = units.Where(u => !occ.Contains(u.Id)).Sum(u => u.Rent ?? 0);
            ViewBag.Active = "Master";
            return View();
        }

        // ============================ PROPERTY PHOTO & DOCUMENT GALLERY ============================
        [HttpGet]
        public ActionResult Gallery(long? id)
        {
            var allProps = db.PropertyMains.Select(p => new { p.Id, p.Name }).ToList();
            ViewBag.Properties = J(allProps.Select(p => new { id = p.Id, name = p.Name ?? "-" }).ToList());
            if (id == null || id == 0) id = allProps.Select(p => (long?)p.Id).FirstOrDefault();
            ViewBag.SelectedId = id ?? 0;
            ViewBag.PName = db.PropertyMains.Where(p => p.Id == id).Select(p => p.Name).FirstOrDefault() ?? "-";
            if (id != null && id != 0)
            {
                var imgs = db.PropertyImages.Where(i => i.PropertyID == id && i.Status == 0).Select(i => new { i.ID, i.FileName }).ToList();
                ViewBag.Images = J(imgs.Select(i => new { id = i.ID, file = i.FileName ?? "", img = IsImage(i.FileName) }).ToList());
                ViewBag.Count = imgs.Count;
            }
            ViewBag.Active = "Master";
            return View();
        }

        [HttpPost]
        public ActionResult UploadPhotos(long PropertyID)
        {
            var files = Request.Form.Files;
            if (files != null && files.Count > 0 && PropertyID > 0)
            {
                var folder = LegacyWeb.MapPath("~/uploads/PropertyImages/Property_" + PropertyID + "/");
                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
                foreach (var file in files)
                {
                    if (file == null || file.Length == 0) continue;
                    var name = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    file.SaveAs(Path.Combine(folder, name));
                    db.PropertyImages.Add(new PropertyImage { PropertyID = PropertyID, FileName = name, Status = 0 });
                }
                db.SaveChanges();
            }
            return RedirectToAction("Gallery", new { id = PropertyID });
        }

        [HttpGet]
        public ActionResult DeletePhoto(long id)
        {
            var img = db.PropertyImages.FirstOrDefault(i => i.ID == id);
            long pid = 0;
            if (img != null) { pid = img.PropertyID; img.Status = 1; db.SaveChanges(); }
            return RedirectToAction("Gallery", new { id = pid });
        }

        [HttpGet]
        public ActionResult Photo(long id)
        {
            var img = db.PropertyImages.Where(i => i.ID == id).Select(i => new { i.PropertyID, i.FileName }).FirstOrDefault();
            if (img == null || string.IsNullOrEmpty(img.FileName)) return NotFound();
            var path = LegacyWeb.MapPath("~/uploads/PropertyImages/Property_" + img.PropertyID + "/" + img.FileName);
            if (!System.IO.File.Exists(path)) return NotFound();
            var ext = Path.GetExtension(img.FileName).ToLowerInvariant();
            string ct = ext == ".png" ? "image/png" : ext == ".gif" ? "image/gif" : ext == ".webp" ? "image/webp"
                      : ext == ".pdf" ? "application/pdf" : (ext == ".jpg" || ext == ".jpeg") ? "image/jpeg" : "application/octet-stream";
            return PhysicalFile(path, ct);
        }

        private static bool IsImage(string f)
        {
            if (string.IsNullOrEmpty(f)) return false;
            var e = Path.GetExtension(f).ToLowerInvariant();
            return e == ".jpg" || e == ".jpeg" || e == ".png" || e == ".gif" || e == ".webp";
        }

        public class LocRow
        {
            public string location { get; set; }
            public int properties { get; set; }
            public int units { get; set; }
            public int occupied { get; set; }
            public int vacant { get; set; }
            public int pct { get; set; }
            public decimal rent { get; set; }
            public decimal expense { get; set; }
        }
        public class RenewItem
        {
            public string type { get; set; }
            public string title { get; set; }
            public string sub { get; set; }
            public string date { get; set; }
            public int days { get; set; }
            public decimal amount { get; set; }
            public string link { get; set; }
            public int tier { get; set; }
        }

        public class PLRow
        {
            public long id { get; set; }
            public string name { get; set; }
            public string city { get; set; }
            public string ptype { get; set; }
            public decimal income { get; set; }
            public decimal billed { get; set; }
            public decimal expense { get; set; }
            public decimal net { get; set; }
            public int units { get; set; }
            public int occupied { get; set; }
            public int vacant { get; set; }
            public int margin { get; set; }
        }
    }
}
