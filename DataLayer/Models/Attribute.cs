using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLayer.Models
{
    public class DbPrimaryKeyAttribute : Attribute
    {

    }

    public class DbForeignKeyAttribute : Attribute
    {

    }
    public class DbColumnNameAttribute : Attribute
    {
        public string Name { get; set; }
        public DbColumnNameAttribute(string name)
        {
            this.Name = name;
        }
    }
}
