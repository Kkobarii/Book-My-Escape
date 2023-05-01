using DataLayer.Models;
using DataLayer;
using Microsoft.AspNetCore.Mvc;
using WebApp.Models;
using WebApp;

namespace WebApp.Controllers
{
    public class RegisterController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Index(RegisterViewModel form)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            var result = Database.Select<User>("Email", form.Email);

            if (result.Count != 0)
            {
                ViewBag.Message = $"This email address is already registered";
                return View();
            }

            string password = Encryption.Encrypt(form.Password);
            string repeatPassword = Encryption.Encrypt(form.RepeatPassword);

            if (password != repeatPassword)
            {
                ViewBag.Message = $"Passwords do not match";
                return View();
            }

            User user = new User()
            {
                Email = form.Email,
                FirstName = form.FirstName,
                LastName = form.LastName,
                Password = password,
                PhoneNumber = form.PhoneNumber
            };

            Database.Insert(user);

            LoggedInSingleton.Instance.LoggedInUser = user;

            return Redirect("Home");
        }
    }
}
