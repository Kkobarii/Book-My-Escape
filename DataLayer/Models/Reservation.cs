using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLayer.Models
{
    public class Reservation
    {
        [DbPrimaryKey]
        public int Id { get; set; }
        [DbForeignKey, DbColumnName("UserId")]
        public User User { get; set; }
        [DbForeignKey, DbColumnName("RoomId")]
        public Room Room { get; set; }
        public DateTime CheckIn { get; set; }
        public DateTime CheckOut { get; set; }
    }
}
