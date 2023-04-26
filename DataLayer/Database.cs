using DataLayer.Models;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DataLayer
{
    public class Database
    {
        private const string _connectionString = "Data Source=database.db;";
        private const string tab = "    ";

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

        private static string getPropertyType(PropertyInfo property)
        {
            if (CheckAttribute<DbForeignKeyAttribute>(property))
            {
                return "INTEGER";
            }
            else if (property.PropertyType == typeof(int) || property.PropertyType == typeof(int?))
            {
                return "INTEGER";
            }
            else if (property.PropertyType == typeof(long) || property.PropertyType == typeof(long?))
            {
                return "INTEGER";
            }
            else if (property.PropertyType == typeof(string))
            {
                return "TEXT";
            }
            else if (property.PropertyType == typeof(decimal) || property.PropertyType == typeof(decimal?))
            {
                return "REAL";
            }
            else if (property.PropertyType == typeof(bool) || property.PropertyType == typeof(bool?))
            {
                return "INTEGER";
            }
            else if (property.PropertyType == typeof(DateTime) || property.PropertyType == typeof(DateTime?))
            {
                return "TEXT";
            }
            else
            {
                throw new Exception($"Unknown type: {property.Name} {property.PropertyType}");
            }
        }
        
        private static string getSqlParams(PropertyInfo property)
        {
            List<string> sqlParams = new List<string>();

            sqlParams.Add(getPropertyName(property));
            sqlParams.Add(getPropertyType(property));

            if (CheckAttribute<DbPrimaryKeyAttribute>(property))
                sqlParams.Add("not null");
            else if (!CheckNullable(property))
                sqlParams.Add("not null");

            return String.Join(' ', sqlParams);
        }

        private static int getLastId()
        {
            return Convert.ToInt32(ExecuteScalar("SELECT last_insert_rowid()")!);
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

        public static DataTable ExecuteQuery(string query)
        {
            return ExecuteQuery(query, new Dictionary<string, object>());
        }

        public static DataTable ExecuteQuery(string query, Dictionary<string, object> parameters)
        {
            using (SqliteConnection conn = new SqliteConnection(_connectionString))
            {
                conn.Open();
                using (SqliteCommand command = conn.CreateCommand())
                {
                    command.CommandText = query;
                    string text = command.CommandText;

                    foreach (KeyValuePair<string, object> param in parameters)
                    {
                        command.Parameters.AddWithValue(param.Key, param.Value);
                        text = text.Replace(param.Key, param.Value.ToString());
                    }

                    Console.WriteLine("Executing query:");
                    Console.WriteLine(text);

                    DataTable result = new DataTable();
                    result.Load(command.ExecuteReader());

                    return result;
                }
            }
        }

        public static void ExecuteNonQuery(string query)
        {
            ExecuteNonQuery(query, new Dictionary<string, object>());
        }

        public static void ExecuteNonQuery(string query, Dictionary<string, object> parameters)
        {
            using (SqliteConnection conn = new SqliteConnection(_connectionString))
            {
                conn.Open();
                using (SqliteCommand command = conn.CreateCommand())
                {
                    command.CommandText = query;
                    string text = command.CommandText;

                    foreach (KeyValuePair<string, object> param in parameters)
                    {
                        command.Parameters.AddWithValue(param.Key, param.Value);
                        text = text.Replace(param.Key, param.Value.ToString());
                    }

                    Console.WriteLine("Executing non query:");
                    Console.WriteLine(text);

                    command.ExecuteNonQuery();
                }
            }
        }

        public static object? ExecuteScalar(string query)
        {
            return ExecuteScalar(query, new Dictionary<string, object>());
        }

        public static object? ExecuteScalar(string query, Dictionary<string, object> parameters)
        {
            object? result;

            using (SqliteConnection conn = new SqliteConnection(_connectionString))
            {
                conn.Open();
                using (SqliteCommand command = conn.CreateCommand())
                {
                    command.CommandText = query;
                    string text = command.CommandText;

                    foreach (KeyValuePair<string, object> param in parameters)
                    {
                        command.Parameters.AddWithValue(param.Key, param.Value);
                        text = text.Replace(param.Key, param.Value.ToString());
                    }

                    Console.WriteLine("Executing scalar:");
                    Console.WriteLine(text);

                    result = command.ExecuteScalar();
                }
            }

            return result;
        }

        public static void DropTableIfExists<T>()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("drop table if exists ");
            stringBuilder.Append(typeof(T).Name);
            stringBuilder.AppendLine(";");
            ExecuteNonQuery(stringBuilder.ToString());
        }

        public static void CreateTable<T>()
        {
            Type type = typeof(T);
            var properties = type.GetProperties();

            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("create table if not exists ");
            stringBuilder.Append(type.Name);
            stringBuilder.AppendLine(" (");

            List<string> lines = new List<string>();
            List<PropertyInfo> primaryKeys = new List<PropertyInfo>();
            List<PropertyInfo> foreignKeys = new List<PropertyInfo>();

            foreach (var property in properties)
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
                StringBuilder primary = new StringBuilder();
                primary.Append(tab + "primary key (");
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

            ExecuteNonQuery(stringBuilder.ToString());
        }

        public static void Insert<T>(T entry)
        {
            Type type = typeof(T);
            var properties = type.GetProperties();
            properties = properties.Where(x => !CheckAttribute<DbPrimaryKeyAttribute>(x)).ToArray();

            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.Append("insert into ");
            stringBuilder.Append(type.Name);
            stringBuilder.Append(" (");

            stringBuilder.Append(string.Join(", ", properties.Select(x => getPropertyName(x))));

            stringBuilder.Append(")\nvalues (");

            List<object> values = new List<object>();
            Dictionary<string, object> attributes = new Dictionary<string, object>();

            foreach (var property in properties)
            {
                var value = property.GetValue(entry);
                values.Add("@" + getPropertyName(property));
                attributes["@" + getPropertyName(property)] = value ?? DBNull.Value;
            }

            stringBuilder.Append(string.Join(", ", values));

            stringBuilder.AppendLine(");");

            ExecuteNonQuery(stringBuilder.ToString(), attributes);

            var idProperty = type.GetProperties().Where(x => CheckAttribute<DbPrimaryKeyAttribute>(x)).ToArray()[0];
            var id = getLastId();

            idProperty.SetValue(entry, id);
        }

        public static void Update<T>(T entry)
        {
            Type type = typeof(T);
            var properties = type.GetProperties();

            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.Append("update ");
            stringBuilder.Append(type.Name);
            stringBuilder.Append("\nset\n");

            List<string> lines = new List<string>();
            Dictionary<string, object> attributes = new Dictionary<string, object>();

            foreach (var property in properties)
            {
                if (CheckAttribute<DbPrimaryKeyAttribute>(property))
                    continue;

                var value = property.GetValue(entry);
                lines.Add($"{tab}{property.Name} = @{property.Name}");
                attributes["@" + getPropertyName(property)] = value ?? DBNull.Value;
            }

            stringBuilder.Append(string.Join(",\n", lines));
            stringBuilder.Append("\nwhere ");

            var idProperty = type.GetProperties().Where(x => CheckAttribute<DbPrimaryKeyAttribute>(x)).ToArray()[0];
            stringBuilder.Append($"{idProperty.Name} = @{idProperty.Name};");
            attributes["@" + getPropertyName(idProperty)] = idProperty.GetValue(entry) ?? DBNull.Value;

            ExecuteNonQuery(stringBuilder.ToString(), attributes);
        }

        public static void Delete<T>(T entry)
        {
            Type type = typeof(T);
            var properties = type.GetProperties();

            StringBuilder stringBuilder = new StringBuilder();
            Dictionary<string, object> attributes = new Dictionary<string, object>();

            stringBuilder.Append("delete from ");
            stringBuilder.Append(type.Name);
            stringBuilder.Append("\nwhere ");

            var idProperty = type.GetProperties().Where(x => CheckAttribute<DbPrimaryKeyAttribute>(x)).ToArray()[0];
            stringBuilder.Append($"{idProperty.Name} = @{idProperty.Name};");
            attributes["@" + getPropertyName(idProperty)] = idProperty.GetValue(entry) ?? DBNull.Value;

            ExecuteNonQuery(stringBuilder.ToString(), attributes);
        }

        public static List<T> Select<T>(int id) where T : new()
        {
            var idProperty = typeof(T).GetProperties().Where(x => CheckAttribute<DbPrimaryKeyAttribute>(x)).ToArray()[0];
            Dictionary<string, object> attributes = new Dictionary<string, object>();
            attributes.Add(idProperty.Name, id);
            return Select<T>(attributes);
        }

        public static List<T> Select<T>(string propertyName, object value) where T : new()
        {
            Dictionary<string, object> attributes = new Dictionary<string, object>();
            attributes.Add(propertyName, value);
            return Select<T>(attributes);
        }

        public static List<T> Select<T>(Dictionary<string, object> parameters) where T: new()
        {
            Type type = typeof(T);
            var properties = type.GetProperties();

            List<T> result = new List<T>();
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.Append("select ");

            // todo linq
            List<string> pain = new List<string>();
            foreach (var item in properties)
            {
                pain.Add(item.Name);
            }

            stringBuilder.Append(string.Join(", ", pain));
            stringBuilder.Append(" \nfrom ");
            stringBuilder.Append(type.Name);
            stringBuilder.Append(" \nwhere\n");

            List<string> lines = new List<string>();
            Dictionary<string, object> attributes = new Dictionary<string, object>();

            foreach (KeyValuePair<string, object> param in parameters)
            {
                lines.Add($"{tab}{param.Key} = @{param.Key}");
                attributes["@" + param.Key] = param.Value ?? DBNull.Value;
            }

            stringBuilder.Append(string.Join(" and \n", lines));
            stringBuilder.AppendLine(";");


            DataTable table = ExecuteQuery(stringBuilder.ToString(), attributes);

            foreach (DataRow row in table.Rows)
            {
                T entry = new T();

                foreach (var property in properties)
                {
                    object? value = (object)row[getPropertyName(property)];

                    if (value == DBNull.Value)
                        property.SetValue(entry, null);
                    else if (property.PropertyType == typeof(int) || property.PropertyType == typeof(int?))
                        property.SetValue(entry, Convert.ToInt32(value));
                    else if (property.PropertyType == typeof(bool))
                        property.SetValue(entry, Convert.ToBoolean(value));
                    else
                        property.SetValue(entry, value);
                }

                result.Add(entry);
            }

            return result;
        }
    }
}
