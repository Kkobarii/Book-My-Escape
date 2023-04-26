﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLayer.Models
{
    public class Room
    {
        [DbPrimaryKey]
        public int? Id { get; set; }
        public int Capacity { get; set; }
        public double? Price { get; set; }
        public bool IsAvailable { get; set; }

        public override string? ToString()
        {
            return $"Room {Id}:\n  Capacity: {Capacity}\n  Price: {Price}\n  Is available? {(IsAvailable ? "true" : "false")}";
        }
    }
}
