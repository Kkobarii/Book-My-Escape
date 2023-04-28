using Microsoft.Data.Sqlite;
using System.Data;

namespace DataLayer
{
    public class Database
    {
        private static long getLastId()
        {
            return Convert.ToInt32(ExecuteScalar("SELECT last_insert_rowid()\n")!);
        }

        public static DataTable ExecuteQuery(string query)
        {
            return ExecuteQuery(query, new Dictionary<string, object>());
        }
        public static DataTable ExecuteQuery((string, Dictionary<string, object>) query)
        {
            return ExecuteQuery(query.Item1, query.Item2);
        }
        public static DataTable ExecuteQuery(string query, Dictionary<string, object> parameters)
        {
            using (SqliteConnection conn = new SqliteConnection(GlobalConfig.ConnectionString))
            {
                conn.Open();
                using (SqliteCommand command = conn.CreateCommand())
                {
                    command.CommandText = query;

                    foreach (KeyValuePair<string, object> param in parameters)
                    {
                        command.Parameters.AddWithValue(param.Key, param.Value);
                    }

                    SqlLog.Log("query", query, parameters);

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
        public static void ExecuteNonQuery((string, Dictionary<string, object>) query)
        {
            ExecuteNonQuery(query.Item1, query.Item2);
        }
        public static void ExecuteNonQuery(string query, Dictionary<string, object> parameters)
        {
            using (SqliteConnection conn = new SqliteConnection(GlobalConfig.ConnectionString))
            {
                conn.Open();
                using (SqliteCommand command = conn.CreateCommand())
                {
                    command.CommandText = query;

                    foreach (KeyValuePair<string, object> param in parameters)
                    {
                        command.Parameters.AddWithValue(param.Key, param.Value);
                    }

                    SqlLog.Log("non-query", query, parameters);

                    command.ExecuteNonQuery();
                }
            }
        }

        public static object? ExecuteScalar(string query)
        {
            return ExecuteScalar(query, new Dictionary<string, object>());
        }
        public static object? ExecuteScalar((string, Dictionary<string, object>) query)
        {
            return ExecuteScalar(query.Item1, query.Item2);
        }
        public static object? ExecuteScalar(string query, Dictionary<string, object> parameters)
        {
            object? result;

            using (SqliteConnection conn = new SqliteConnection(GlobalConfig.ConnectionString))
            {
                conn.Open();
                using (SqliteCommand command = conn.CreateCommand())
                {
                    command.CommandText = query;

                    foreach (KeyValuePair<string, object> param in parameters)
                    {
                        command.Parameters.AddWithValue(param.Key, param.Value);
                    }

                    SqlLog.Log("scalar", query, parameters);

                    result = command.ExecuteScalar();
                }
            }

            return result;
        }


        public static void CreateDatabase()
        {
            if (!File.Exists("database.db"))
            {
                ExecuteNonQuery("");
            }
        }
        public static void DropDatabase()
        {
            if (File.Exists("database.db"))
            {
                File.Delete("database.db");
            }
        }

        public static void DropTableIfExists<T>()
        {
            var query = QueryBuilder.GetDropTableQuery<T>();
            ExecuteNonQuery(query);
        }

        public static void CreateTable<T>()
        {
            var query = QueryBuilder.GetCreateTableQuery<T>();
            ExecuteNonQuery(query);
        }

        public static void Insert<T>(T entry)
        {
            if (Mapper.GetPrimaryIdValue(entry) != null)
                throw new Exception("Cannot insert an entry that already has a primary key value");
            
            var query = QueryBuilder.GetInsertQuery(entry);
            ExecuteNonQuery(query);
            
            var id = getLastId();
            Mapper.SetPrimaryIdValue(entry, id);
        }

        public static void Update<T>(T entry)
        {
            if (Mapper.GetPrimaryIdValue(entry) == null)
                throw new Exception("Cannot update an entry that does not have a primary key value");

            var query = QueryBuilder.GetUpdateQuery(entry);
            ExecuteNonQuery(query);
        }

        public static void Delete<T>(T entry)
        {
            if (Mapper.GetPrimaryIdValue(entry) == null)
                throw new Exception("Cannot delete an entry that does not have a primary key value");

            var query = QueryBuilder.GetDeleteQuery(entry);            
            ExecuteNonQuery(query);
        }

        public static List<T> Select<T>() where T : new()
        {
            return Select<T>(new Dictionary<string, object>());
        }
        public static List<T> Select<T>(long id) where T : new()
        {
            Dictionary<string, object> attributes = new Dictionary<string, object>();
            attributes.Add(Mapper.GetPrimaryIdName<T>(), id);
            return Select<T>(attributes);
        }

        public static List<T> Select<T>(string propertyName, object value) where T : new()
        {
            Dictionary<string, object> attributes = new Dictionary<string, object>();
            attributes.Add(propertyName, value);
            return Select<T>(attributes);
        }

        public static List<T> Select<T>(Dictionary<string, object> parameters) where T : new()
        {
            var query = QueryBuilder.GetSelectQuery<T>(parameters);            
            DataTable table = ExecuteQuery(query);
            var result = new List<T>();

            foreach (DataRow row in table.Rows)
            {
                T entry = Mapper.MapRowToT<T>(row);
                result.Add(entry);
            }

            return result;
        }
    }
}
