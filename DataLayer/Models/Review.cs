using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLayer.Models
{
    public class Review
    {
        [DbPrimaryKey]
        public long? Id { get; set; }
        [DbForeignKey, DbColumnName("UserId")]
        public User User { get; set; }
        [DbForeignKey, DbColumnName("RoomId")]
        public Room Room { get; set; }
        public string Text { get; set; }
        public int Rating { get; set; }

        public override string? ToString()
        {
            return $"{Id}: {Room} by {User}";
        }
    }
}
