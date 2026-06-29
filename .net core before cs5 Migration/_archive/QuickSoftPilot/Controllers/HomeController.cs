using Microsoft.AspNetCore.Mvc;

namespace QuickSoftPilot.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        if (User.Identity != null && User.Identity.IsAuthenticated)
            return RedirectToAction("Index", "ItemCategory");
        return RedirectToAction("Login", "Account");
    }
}
