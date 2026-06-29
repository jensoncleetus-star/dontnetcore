using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using QuickSoftPilot.Legacy;
using QuickSoftPilot.ViewModels;
using System.Linq.Dynamic.Core;

namespace QuickSoftPilot.Controllers
{
    // Full CRUD on the REAL ItemCategories table in emirtechlatest.
    [Authorize]
    public class ItemCategoryController : Controller
    {
        private readonly LegacyDbContext _db;
        public ItemCategoryController(LegacyDbContext db) { _db = db; }

        public IActionResult Index() => View();

        [HttpPost]
        public IActionResult GetItemCategory(long? ddlCategory, long? Parent)
        {
            var form = Request.Form;
            string search = form["search[value]"];
            string draw = form["draw"];
            int start = int.TryParse(form["start"], out var s) ? s : 0;
            int length = int.TryParse(form["length"], out var l) ? l : 10;
            string sortColumn = form["columns[" + form["order[0][column]"] + "][name]"];
            string sortDir = form["order[0][dir]"];

            var q = from a in _db.ItemCategories
                    join b in _db.ItemCategories on a.Parent equals b.ItemCategoryID into g
                    from b in g.DefaultIfEmpty()
                    select new { a.ItemCategoryID, a.ItemCategoryName, a.Description, a.Parent, ParentName = b.ItemCategoryName };

            if (!string.IsNullOrWhiteSpace(search))
                q = q.Where(p => p.ItemCategoryName.Contains(search));

            int total = q.Count();
            q = !string.IsNullOrEmpty(sortColumn)
                ? q.OrderBy(sortColumn + " " + (sortDir == "desc" ? "desc" : "asc"))
                : q.OrderBy(x => x.ItemCategoryID);

            var data = q.Skip(start).Take(length).ToList();
            return Json(new { draw, recordsFiltered = total, recordsTotal = total, data });
        }

        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Parents = ParentList(null);
            return View(new ItemCategoryViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(ItemCategoryViewModel vm)
        {
            if (_db.ItemCategories.Any(c => c.ItemCategoryName == vm.ItemCategoryName))
                ModelState.AddModelError(nameof(vm.ItemCategoryName), "A category with that name already exists.");
            if (!ModelState.IsValid) { ViewBag.Parents = ParentList(vm.Parent); return View(vm); }

            _db.ItemCategories.Add(new LegacyItemCategory
            {
                ItemCategoryName = vm.ItemCategoryName.Trim(),
                Description = vm.Description,
                Parent = vm.Parent ?? 0,
                Editable = 1
            });
            _db.SaveChanges();
            TempData["msg"] = "Item category added to the real database (emirtechlatest).";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Edit(long? id)
        {
            if (id == null) return BadRequest();
            var info = _db.ItemCategories.Find(id);
            if (info == null) return NotFound();
            ViewBag.Parents = ParentList(info.Parent, info.ItemCategoryID);
            return View(new ItemCategoryViewModel
            {
                ItemCategoryID = info.ItemCategoryID,
                ItemCategoryName = info.ItemCategoryName,
                Parent = info.Parent,
                Description = info.Description
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(ItemCategoryViewModel vm)
        {
            if (!ModelState.IsValid) { ViewBag.Parents = ParentList(vm.Parent, vm.ItemCategoryID); return View(vm); }
            var info = _db.ItemCategories.Find(vm.ItemCategoryID);
            if (info == null) return NotFound();

            info.ItemCategoryName = vm.ItemCategoryName.Trim();
            info.Description = vm.Description;
            info.Parent = vm.Parent ?? 0;
            _db.SaveChanges();
            TempData["msg"] = "Item category updated in the real database.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(long id)
        {
            if (_db.ItemCategories.Any(c => c.Parent == id))
                return Json(new { status = false, message = "Cannot delete: sub-categories exist." });
            var cat = _db.ItemCategories.Find(id);
            if (cat == null) return Json(new { status = false, message = "Category not found." });
            try
            {
                _db.ItemCategories.Remove(cat);
                _db.SaveChanges();
            }
            catch
            {
                return Json(new { status = false, message = "Cannot delete: items are linked to this category." });
            }
            return Json(new { status = true, message = "Item category deleted from the real database." });
        }

        private SelectList ParentList(long? selected, long? excludeId = null)
        {
            var items = _db.ItemCategories
                .Where(c => excludeId == null || c.ItemCategoryID != excludeId)
                .OrderBy(c => c.ItemCategoryName)
                .Select(c => new { c.ItemCategoryID, c.ItemCategoryName })
                .ToList();
            return new SelectList(items, "ItemCategoryID", "ItemCategoryName", selected);
        }
    }
}
