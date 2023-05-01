using Microsoft.AspNetCore.Mvc;
using DataLayer;
using DataLayer.Models;
using WebApp.Models;

namespace WebApp.Controllers
{
    public class LoginController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Index(LoginViewModel form)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            var result = Database.Select<User>("Email", form.Email);

            if (result.Count == 0)
            {
                ViewBag.Message = $"User {form.Email} not found";
                return View();
            }

            User user = result.First();
            string password = Encryption.Encrypt(form.Password);

            if (user.Password != password)
            {
                ViewBag.Message = $"Wrong password for user {form.Email}";
                return View();
            }

            LoggedInSingleton.Instance.LoggedInUser = user;

            return Redirect("Home");
        }
    }
}
