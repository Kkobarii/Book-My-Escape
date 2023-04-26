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

            //Database.Insert(user);
            //Console.WriteLine(user.Id);

            //Database.Update(user);
            //Database.Delete(user);

            Dictionary<string, object> data = new Dictionary<string, object>();
            data["Password"] = "aaa";
            data["LastName"] = "Mrkvička";

            //var res = Database.Select<User>(1);
            //var res = Database.Select<User>("Password" , "aaa");
            var res = Database.Select<User>(data);

            foreach (var item in res)
            {
                Console.WriteLine(item);
            }

        }
    }
}