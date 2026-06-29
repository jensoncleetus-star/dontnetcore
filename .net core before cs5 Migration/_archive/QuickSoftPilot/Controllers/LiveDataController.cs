using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuickSoftPilot.Legacy;
using System.Linq.Dynamic.Core;

namespace QuickSoftPilot.Controllers
{
    // Reads the REAL QuickNet database (emirtechlatest) through the read-only LegacyDbContext.
    [Authorize]
    public class LiveDataController : Controller
    {
        private readonly LegacyDbContext _legacy;
        public LiveDataController(LegacyDbContext legacy) { _legacy = legacy; }

        public IActionResult Index()
        {
            ViewBag.Categories = _legacy.ItemCategories.Count();
            ViewBag.Quotations = _legacy.Quotations.Count();
            ViewBag.Customers = _legacy.Customers.Count();
            return View();
        }

        [HttpPost]
        public IActionResult GetCategories()
        {
            var form = Request.Form;
            string search = form["search[value]"];
            string draw = form["draw"];
            int start = int.TryParse(form["start"], out var s) ? s : 0;
            int length = int.TryParse(form["length"], out var l) ? l : 10;
            string sortColumn = form["columns[" + form["order[0][column]"] + "][name]"];
            string sortDir = form["order[0][dir]"];

            var q = from a in _legacy.ItemCategories
                    join b in _legacy.ItemCategories on a.Parent equals b.ItemCategoryID into g
                    from b in g.DefaultIfEmpty()
                    select new { a.ItemCategoryID, a.ItemCategoryName, a.Description, ParentName = b.ItemCategoryName };

            if (!string.IsNullOrWhiteSpace(search))
                q = q.Where(p => p.ItemCategoryName.Contains(search));

            int total = q.Count();
            q = !string.IsNullOrEmpty(sortColumn)
                ? q.OrderBy(sortColumn + " " + (sortDir == "desc" ? "desc" : "asc"))
                : q.OrderBy(x => x.ItemCategoryID);

            var data = q.Skip(start).Take(length).ToList();
            return Json(new { draw, recordsFiltered = total, recordsTotal = total, data });
        }

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

            var q = from quot in _legacy.Quotations
                    join cust in _legacy.Customers on quot.Customer equals cust.CustomerID into gj
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

        // ---- WRITE operations against the REAL ItemCategories table (emirtechlatest) ----

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateCategory(string name, long? parent, string description)
        {
            if (string.IsNullOrWhiteSpace(name))
                return Json(new { status = false, message = "Category name is required." });
            if (_legacy.ItemCategories.Any(c => c.ItemCategoryName == name.Trim()))
                return Json(new { status = false, message = "A category with that name already exists." });

            var cat = new LegacyItemCategory
            {
                ItemCategoryName = name.Trim(),
                Parent = parent ?? 0,
                Description = description,
                Editable = 1
            };
            _legacy.ItemCategories.Add(cat);
            _legacy.SaveChanges();   // writes a real row to emirtechlatest
            return Json(new { status = true, message = $"Created category #{cat.ItemCategoryID} in the real database.", id = cat.ItemCategoryID });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditCategory(long id, string name, string description)
        {
            var cat = _legacy.ItemCategories.Find(id);
            if (cat == null) return Json(new { status = false, message = "Category not found." });
            if (string.IsNullOrWhiteSpace(name)) return Json(new { status = false, message = "Name is required." });

            cat.ItemCategoryName = name.Trim();
            cat.Description = description;
            _legacy.SaveChanges();
            return Json(new { status = true, message = "Category updated in the real database." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteCategory(long id)
        {
            var cat = _legacy.ItemCategories.Find(id);
            if (cat == null) return Json(new { status = false, message = "Category not found." });

            _legacy.ItemCategories.Remove(cat);
            _legacy.SaveChanges();
            return Json(new { status = true, message = "Category deleted from the real database." });
        }
    }
}
