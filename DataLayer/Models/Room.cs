using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLayer.Models
{
    public class Room
    {
        [DbPrimaryKey]
        public long? Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public int Difficulty { get; set; }
        public int Capacity { get; set; }
        public double Price { get; set; }

        public override string? ToString()
        {
            return $"{Name}";
        }
    }
}
