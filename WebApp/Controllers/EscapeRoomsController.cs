using DataLayer;
using DataLayer.Models;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers
{
    public class EscapeRoomsController : Controller
    {
        public IActionResult Index()
        {
            ViewBag.Rooms = Database.Select<Room>();
			return View();
        }

        public IActionResult Detail(long id)
        {
			ViewBag.Room = Database.Select<Room>(id).First();
            ViewBag.Reviews = Database.Select<Review>("RoomId", id);
			return View();
		}
    }
}
