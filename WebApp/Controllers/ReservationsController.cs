using DataLayer;
using DataLayer.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text.Json.Serialization;
using WebApp.Models;

namespace WebApp.Controllers
{
    public class ReservationsController : Controller
    {
        public object StringStream { get; private set; }

        private int GetFreeSpacesCount(Room room, DateTime checkIn, DateTime checkOut)
        {
            var reservations = Database.Select<Reservation>("RoomId", room.Id!);
            int freeSpaces = room.Capacity;
            foreach (var reservation in reservations)
            {
                if (reservation.CheckIn < checkOut && reservation.CheckOut > checkIn)
                {
                    freeSpaces -= reservation.PlayerCount;
                }
            }
            return freeSpaces;
        }

        public IActionResult Index()
        {
            if (LoggedInSingleton.Instance.LoggedInUser == null)
            {
                return RedirectToAction("Index", "Home");
            }

            ViewBag.Reservations = Database.Select<Reservation>("UserId", LoggedInSingleton.Instance.LoggedInUser.Id!); ;
            return View();
        }

        public IActionResult CreateReservation(long id)
        {
            if (LoggedInSingleton.Instance.LoggedInUser == null)
            {
                return RedirectToAction("Index", "Home");
            }

            ViewBag.Room = Database.Select<Room>(id).First();

            return View();
        }

        [HttpPost]
        public IActionResult CreateReservation(ReservationViewModel form, long id)
        {
			var room = Database.Select<Room>(id).First();
			ViewBag.Room = room;

			if (!ModelState.IsValid)
            {
				return View();
            }
			if (LoggedInSingleton.Instance.LoggedInUser == null || room == null)
            {
                return RedirectToAction("Index", "Home");
            }
            if (form == null)
            {
				ViewBag.Message = $"Please fill in everything";
				return View();
			}
            if (form.CheckIn >= form.CheckOut)
            {
				ViewBag.Message = $"Check-in date must be before check-out date";
				return View();
			}
            if (form.PlayerCount > GetFreeSpacesCount(room, form.CheckIn, form.CheckOut))
            {
                ViewBag.Message = $"Room only has space for {GetFreeSpacesCount(room, form.CheckIn, form.CheckOut)} players";
                return View();
            }

            User user = LoggedInSingleton.Instance.LoggedInUser!;

            Reservation reservation = new Reservation()
            {
                Room = room,
                User = user,
                CheckIn = form.CheckIn,
                CheckOut = form.CheckOut,
                PlayerCount = form.PlayerCount
            };

            Database.Insert(reservation);

            return RedirectToAction("Index", "Reservations");
        }

        public IActionResult DeleteReservation(long id)
        {
			if (LoggedInSingleton.Instance.LoggedInUser == null)
            {
				return RedirectToAction("Index", "Home");
			}

            var reservation = Database.Select<Reservation>(id).First();

            Database.Delete(reservation);

			return RedirectToAction("Index", "Reservations");
		}

		public IActionResult CreateReview(long id)
		{
			if (LoggedInSingleton.Instance.LoggedInUser == null)
			{
				return RedirectToAction("Index", "Home");
			}

			ViewBag.Reservation = Database.Select<Reservation>(id).First();
			return View();
		}

		[HttpPost]
        public IActionResult CreateReview(ReviewViewModel form, long id) 
        { 
            ViewBag.Reservation = Database.Select<Reservation>(id).First();

			if (!ModelState.IsValid)
            {
				return View();
			}
            if (LoggedInSingleton.Instance.LoggedInUser == null)
            {
				return RedirectToAction("Index", "Home");
			}
			if (!ModelState.IsValid)
            {
				return RedirectToAction("Index", "Home");
			}

			var room = Database.Select<Reservation>(id).First().Room;
			if (room == null)
            {
				return RedirectToAction("Index", "Home");
			}

			var user = LoggedInSingleton.Instance.LoggedInUser;
			var review = new Review()
            {
				Room = room,
				User = user,
				Rating = form.Rating,
				Text = form.Text
			};

            Thread t = new Thread(() => { Database.Insert(review); });
            t.Start();
			
			return RedirectToAction("Index", "Reservations");
        }
        
        private async Task ExportReservations()
        {
            var reservations = Database.Select<Reservation>("UserId", LoggedInSingleton.Instance.LoggedInUser!.Id!);
            
            if (!Directory.Exists(GlobalConfig.AssetsPath))
            {
                Directory.CreateDirectory(GlobalConfig.AssetsPath);
            }
            string path = GlobalConfig.AssetsPath + "reservations.json";
            
            using var stream = new MemoryStream();
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };

            await JsonSerializer.SerializeAsync(stream, reservations, options);
            stream.Position = 0;
            using var reader = new StreamReader(stream);
            string json = await reader.ReadToEndAsync();

            await System.IO.File.WriteAllTextAsync(path, json);
        }
        
        public async Task<IActionResult> Export()
        {
            if (LoggedInSingleton.Instance.LoggedInUser == null)
            {
                return RedirectToAction("Index", "Home");
            }

            await ExportReservations();
            return RedirectToAction("Index");
        }
    }
}
