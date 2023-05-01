using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult Index()
        {
            if (LoggedInSingleton.Instance.LoggedInUser == null)
            {
                return RedirectToAction("Index", "Login");
            }
            return View();
        }
    }
}
