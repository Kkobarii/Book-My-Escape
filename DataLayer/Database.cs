using DataLayer.Models;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DataLayer
{
    public class Database
    {
        private const string _connectionString = "Data Source=database.db;";

        private static bool CheckAttribute<T>(PropertyInfo property)
        {
            return property.GetCustomAttributes().OfType<T>().Any();
        }

        private static bool CheckNullable(PropertyInfo property)
        {
            return Nullable.GetUnderlyingType(property.PropertyType) != null;
        }

        private static string getPropertyName(PropertyInfo property)
        {
            var customName = property.GetCustomAttribute<DbColumnNameAttribute>()?.Name;
            if (!string.IsNullOrEmpty(customName))
                return customName;
            else
                return property.Name;
        }
        
        private static string getSqlParams(PropertyInfo property)
        {
            List<string> sqlParams = new List<string>();

            sqlParams.Add(getPropertyName(property));

            if (property.PropertyType == typeof(int) || property.PropertyType == typeof(int?))
            {
                sqlParams.Add("INTEGER");
            }
            else if (property.PropertyType == typeof(string))
            {
                sqlParams.Add("TEXT");
            }
            else if (property.PropertyType == typeof(decimal) || property.PropertyType == typeof(decimal?))
            {
                sqlParams.Add("REAL");
            }
            else if (property.PropertyType == typeof(bool) || property.PropertyType == typeof(bool?))
            {
                sqlParams.Add("INTEGER");
            }
            else if (property.PropertyType == typeof(DateTime) || property.PropertyType == typeof(DateTime?))
            {
                sqlParams.Add("TEXT");
            }
            else if (property.PropertyType.IsClass)
            {
                sqlParams.Add("INTEGER");
            }
            else
            {
                throw new Exception($"Unknown type: {property.Name} {property.PropertyType}");
            }

            if (!CheckNullable(property))
                sqlParams.Add("not null");

            return String.Join(' ', sqlParams);
        }

        public static void CreateDatabase()
        {
            using (SqliteConnection conn = new SqliteConnection(_connectionString))
            {
                conn.Open();
                using (SqliteCommand command = conn.CreateCommand())
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        public static void ExecuteQuery(string query)
        {
            Console.WriteLine("Executing query:");
            Console.WriteLine(query);
            
            using (SqliteConnection conn = new SqliteConnection(_connectionString))
            {
                conn.Open();
                using (SqliteCommand command = conn.CreateCommand())
                {
                    command.CommandText = query;
                    command.ExecuteNonQuery();
                }
            }
        }

        public static void DropTableIfExists<T>()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("drop table if exists ");
            stringBuilder.Append(typeof(T).Name);
            stringBuilder.AppendLine(";");
            ExecuteQuery(stringBuilder.ToString());
        }

        public static void CreateTable<T>()
        {
            Type type = typeof(T);
            var props = type.GetProperties();

            string tab = "   ";
            
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("create table if not exists ");
            stringBuilder.Append(type.Name);
            stringBuilder.AppendLine(" (");

            List<string> lines = new List<string>();
            List<PropertyInfo> primaryKeys = new List<PropertyInfo>();
            List<PropertyInfo> foreignKeys = new List<PropertyInfo>();

            foreach (var property in props)
            {
                var propertyParams = getSqlParams(property);

                if (CheckAttribute<DbPrimaryKeyAttribute>(property))
                    primaryKeys.Add(property);
                if (CheckAttribute<DbForeignKeyAttribute>(property))
                    foreignKeys.Add(property);

                lines.Add(tab + propertyParams);
            }

            if (primaryKeys.Any())
            {
                StringBuilder primary = new StringBuilder(tab + "primary key (");
                primary.Append(string.Join(", ", primaryKeys.Select(x => getPropertyName(x))));
                primary.Append(")");

                lines.Add(primary.ToString());
            }

            if (foreignKeys.Any())
            {
                foreach (var key in foreignKeys)
                {
                    StringBuilder foreign = new StringBuilder();

                    foreign.Append(tab + "foreign key (");
                    foreign.Append(getPropertyName(key));
                    foreign.Append(")\n");

                    foreign.Append(tab + tab + "references ");
                    foreign.Append(key.Name);
                    foreign.Append(" (Id)\n");

                    foreign.Append(tab + tab + tab + "on delete cascade\n");
                    foreign.Append(tab + tab + tab + "on update no action");

                    lines.Add(foreign.ToString());
                }
            }

            stringBuilder.Append(string.Join(",\n", lines));
            stringBuilder.AppendLine("\n);");

            ExecuteQuery(stringBuilder.ToString());
        }

    }
}
