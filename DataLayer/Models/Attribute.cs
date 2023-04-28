namespace DataLayer.Models
{
    public class DbPrimaryKeyAttribute : Attribute { }

    public class DbForeignKeyAttribute : Attribute { }

    public class DbColumnNameAttribute : Attribute
    {
        public string Name { get; set; }
        public DbColumnNameAttribute(string name)
        {
            this.Name = name;
        }
    }
}
