using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuickSoftPilot.Legacy;
using QuickSoftPilot.ViewModels;
using System.Linq.Dynamic.Core;

namespace QuickSoftPilot.Controllers
{
    // Reads the REAL Quotations in emirtechlatest (list + details with line items).
    [Authorize]
    public class QuotationController : Controller
    {
        private readonly LegacyDbContext _db;
        public QuotationController(LegacyDbContext db) { _db = db; }

        public IActionResult Index() => View();

        [HttpPost]
        public IActionResult GetQuotations()
        {
            var form = Request.Form;
            string search = form["search[value]"];
            string draw = form["draw"];
            int start = int.TryParse(form["start"], out var s) ? s : 0;
            int length = int.TryParse(form["length"], out var l) ? l : 10;
            string sortColumn = form["columns[" + form["order[0][column]"] + "][name]"];
            string sortDir = form["order[0][dir]"];

            var q = from quot in _db.Quotations
                    join cust in _db.Customers on quot.Customer equals cust.CustomerID into gj
                    from cust in gj.DefaultIfEmpty()
                    select new { quot.QuotationId, quot.BillNo, quot.QuotDate, CustomerName = cust.CustomerName, quot.QuotItems, quot.QuotGrandTotal };

            if (!string.IsNullOrWhiteSpace(search))
                q = q.Where(p => p.BillNo.Contains(search) || p.CustomerName.Contains(search));

            int total = q.Count();
            q = !string.IsNullOrEmpty(sortColumn)
                ? q.OrderBy(sortColumn + " " + (sortDir == "desc" ? "desc" : "asc"))
                : q.OrderByDescending(x => x.QuotationId);

            var data = q.Skip(start).Take(length).ToList().Select(x => new
            {
                x.QuotationId,
                x.BillNo,
                QuotDate = x.QuotDate.ToString("dd-MM-yyyy"),
                x.CustomerName,
                x.QuotItems,
                QuotGrandTotal = x.QuotGrandTotal.ToString("N2")
            });
            return Json(new { draw, recordsFiltered = total, recordsTotal = total, data });
        }

        [HttpGet]
        public IActionResult Details(long id)
        {
            var q = _db.Quotations.FirstOrDefault(x => x.QuotationId == id);
            if (q == null) return NotFound();

            var custName = _db.Customers.Where(c => c.CustomerID == q.Customer).Select(c => c.CustomerName).FirstOrDefault();

            var lines = (from li in _db.QuotationItems
                         where li.Quotation == id
                         join it in _db.Items on li.Item equals it.ItemId into gi
                         from it in gi.DefaultIfEmpty()
                         select new QuotationDetailsLine
                         {
                             ItemName = it.ItemName,
                             ItemQuantity = li.ItemQuantity,
                             ItemUnitPrice = li.ItemUnitPrice,
                             ItemTotalAmount = li.ItemTotalAmount,
                             ItemNote = li.ItemNote
                         }).ToList();

            return View(new QuotationDetailsViewModel
            {
                QuotationId = q.QuotationId,
                BillNo = q.BillNo,
                QuotDate = q.QuotDate,
                CustomerName = custName,
                QuotItems = q.QuotItems,
                QuotItemQuantity = q.QuotItemQuantity,
                QuotSubTotal = q.QuotSubTotal,
                QuotTaxAmount = q.QuotTaxAmount,
                QuotGrandTotal = q.QuotGrandTotal,
                QuotNote = q.QuotNote,
                Lines = lines
            });
        }
    }
}
