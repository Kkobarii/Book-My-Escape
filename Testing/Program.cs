using DataLayer;
using DataLayer.Models;

namespace Testing
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("\n");
            Console.WriteLine("\n");
            Database.CreateDatabase();

            Database.DropTableIfExists<Room>();
            Database.DropTableIfExists<Reservation>();
            Database.DropTableIfExists<User>();

            Database.CreateTable<Room>();
            Database.CreateTable<Reservation>();
            Database.CreateTable<User>();
        }
    }
}