using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLayer.Models
{
    public class User
    {
        [DbPrimaryKey]
        public long? Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public bool IsAdmin { get; set; }

        public override string? ToString()
        {
            return $"User {Id}: {FirstName} {LastName}\n  Email: {Email}\n  Password: {Password} (Do not steal)\n  Is admin? {(IsAdmin ? "true": "false")}";
        }
    }
}
