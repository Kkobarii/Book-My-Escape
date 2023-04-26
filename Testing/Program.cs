using DataLayer;
using DataLayer.Models;

namespace Testing
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("\n");
            Database.CreateDatabase();

            //Database.DropTableIfExists<Room>();
            //Database.DropTableIfExists<Reservation>();
            //Database.DropTableIfExists<User>();

            Database.CreateTable<Room>();
            Database.CreateTable<Reservation>();
            Database.CreateTable<User>();

            User user = new User()
            {
                Id = 2,
                FirstName = "Jožko",
                LastName = "Mrkvička",
                Email = "j.T@vsb.cz",
                IsAdmin = true,
                Password = "aaa"
            };

            Room room = new Room()
            {
                Id = 1,
                Capacity = 5,
                IsAvailable = true,
                Price = 800
            };

            Reservation reservation = new Reservation()
            {
                User = user,
                Room = room,
                CheckIn = new DateTime(2008, 5, 1, 8, 30, 52),
                CheckOut = new DateTime(2008, 5, 2, 8, 30, 52)
            };

            //Database.Insert(reservation);
            //Console.WriteLine(user.Id);

            //Database.Update(user);
            //Database.Delete(user);

            Dictionary<string, object> data = new Dictionary<string, object>();
            data["Password"] = "aaa";
            data["LastName"] = "Mrkvička";

            var res = Database.Select<Reservation>(1);
            //var res = Database.Select<User>("Password" , "aaa");
            //var res = Database.Select<User>(data);

            foreach (var item in res)
            {
                Console.WriteLine(item);
            }

        }
    }
}