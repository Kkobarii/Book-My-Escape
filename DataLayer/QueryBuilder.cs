using DataLayer.Models;
using System.Reflection;
using System.Text;

namespace DataLayer
{
    public class QueryBuilder
    {
        private const string tab = "    ";

        public static (string, Dictionary<string, object>) GetDropTableQuery<T>()
        {
            StringBuilder stringBuilder = new();
            stringBuilder.Append("drop table if exists ");
            stringBuilder.Append(typeof(T).Name);
            stringBuilder.AppendLine(";");

            return (stringBuilder.ToString(), new Dictionary<string, object>());
        }
        public static (string, Dictionary<string, object>) GetCreateTableQuery<T>()
        {
            Type type = typeof(T);
            var properties = type.GetProperties();

            StringBuilder stringBuilder = new();
            stringBuilder.Append("create table if not exists ");
            stringBuilder.Append(type.Name);
            stringBuilder.AppendLine(" (");

            List<string> lines = new();
            List<PropertyInfo> primaryKeys = new();
            List<PropertyInfo> foreignKeys = new();

            foreach (var property in properties)
            {
                var propertyParams = Mapper.GetSqlCreateTableParams(property);

                if (Mapper.CheckAttribute<DbPrimaryKeyAttribute>(property))
                    primaryKeys.Add(property);
                if (Mapper.CheckAttribute<DbForeignKeyAttribute>(property))
                    foreignKeys.Add(property);

                lines.Add(tab + propertyParams);
            }

            if (primaryKeys.Any())
            {
                StringBuilder primary = new();
                primary.Append(tab + "primary key (");
                primary.Append(string.Join(", ", primaryKeys.Select(x => Mapper.GetPropertyName(x))));
                primary.Append(")");

                lines.Add(primary.ToString());
            }

            if (foreignKeys.Any())
            {
                foreach (var key in foreignKeys)
                {
                    StringBuilder foreign = new();
                    
                    MethodInfo method = typeof(Mapper).GetMethods().Where(m => m.Name == "GetPrimaryIdName" && m.GetParameters().Length == 0).First();
                    MethodInfo generic = method.MakeGenericMethod(key.PropertyType);
                    var idPropertyName = (generic.Invoke(null, new object[] { })!).ToString();
                    
                    foreign.Append(tab + "foreign key (");
                    foreign.Append(Mapper.GetPropertyName(key));
                    foreign.Append(")\n");

                    foreign.Append(tab + tab + "references ");
                    foreign.Append(key.Name);
                    foreign.Append($" ({idPropertyName})\n");

                    foreign.Append(tab + tab + tab + "on delete cascade\n");
                    foreign.Append(tab + tab + tab + "on update no action");

                    lines.Add(foreign.ToString());
                }
            }

            stringBuilder.Append(string.Join(",\n", lines));
            stringBuilder.AppendLine("\n);");

            return (stringBuilder.ToString(), new Dictionary<string, object>());
        }
        public static (string, Dictionary<string, object>) GetInsertQuery<T>(T entry)
        {
            Type type = typeof(T);
            var properties = type.GetProperties();
            properties = properties.Where(x => !Mapper.CheckAttribute<DbPrimaryKeyAttribute>(x)).ToArray();
            
            StringBuilder stringBuilder = new();

            stringBuilder.Append("insert into ");
            stringBuilder.Append(type.Name);
            stringBuilder.Append(" (");

            stringBuilder.Append(string.Join(", ", properties.Select(x => Mapper.GetPropertyName(x))));

            stringBuilder.Append(") \nvalues (");

            List<object> values = new();
            Dictionary<string, object> attributes = new();

            foreach (var property in properties)
            {
                var value = property.GetValue(entry);
                values.Add("@" + Mapper.GetPropertyName(property));
                attributes["@" + Mapper.GetPropertyName(property)] = Mapper.GetPropertyValue(entry, property) ?? DBNull.Value;
            }
            
            stringBuilder.Append(string.Join(", ", values));

            stringBuilder.AppendLine(");");

            return (stringBuilder.ToString(), attributes);
        }

        public static (string, Dictionary<string, object>) GetDeleteQuery<T>(T entry)
        {
            Type type = typeof(T);

            StringBuilder stringBuilder = new();

            stringBuilder.Append("delete from ");
            stringBuilder.Append(type.Name);
            stringBuilder.Append(" \nwhere ");

            Dictionary<string, object> attributes = new();

            string idName = Mapper.GetPrimaryIdName<T>();
            long? idValue = Mapper.GetPrimaryIdValue(entry);
            stringBuilder.Append($"{idName} = @ {idName}");
            attributes["@" + idName] = idValue ?? throw new Exception("Primary key is null");

            stringBuilder.AppendLine(";");

            return (stringBuilder.ToString(), attributes);
        }
        public static (string, Dictionary<string, object>) GetUpdateQuery<T>(T entry)
        {
            Type type = typeof(T);
            var properties = type.GetProperties();

            StringBuilder stringBuilder = new();

            stringBuilder.Append("update ");
            stringBuilder.Append(type.Name);
            stringBuilder.Append(" \nset \n");

            List<string> lines = new();
            Dictionary<string, object> attributes = new();

            foreach (var property in properties)
            {
                if (Mapper.CheckAttribute<DbPrimaryKeyAttribute>(property))
                    continue;

                var name = Mapper.GetPropertyName(property);
                var value = Mapper.GetPropertyValue(entry, property);
                
                lines.Add(tab + name + " = @" + name);
                attributes["@" + name] = value ?? DBNull.Value;
            }

            stringBuilder.Append(string.Join(", \n", lines));

            stringBuilder.Append(" \nwhere ");

            string idName = Mapper.GetPrimaryIdName<T>();
            long? idValue = Mapper.GetPrimaryIdValue(entry);            
            stringBuilder.Append(idName + " = @" + idName);
            attributes["@" + idName] = idValue ?? throw new Exception("Primary key is null");

            stringBuilder.AppendLine(";");

            return (stringBuilder.ToString(), attributes);
        }
        public static (string, Dictionary<string, object>) GetSelectQuery<T>(Dictionary<string, object> parameters)
        {
            Type type = typeof(T);
            var properties = type.GetProperties();

            List<T> result = new();
            StringBuilder stringBuilder = new();

            stringBuilder.Append("select ");

            stringBuilder.Append(string.Join(", ", properties.Select(x => Mapper.GetPropertyName(x))));
            stringBuilder.Append(" \nfrom ");
            stringBuilder.Append(type.Name);
            
            if (!parameters.Any())
            {
                stringBuilder.AppendLine(";");
                return (stringBuilder.ToString(), new Dictionary<string, object>());
            }

            stringBuilder.Append(" \nwhere\n");

            List<string> lines = new();
            Dictionary<string, object> attributes = new();

            foreach (KeyValuePair<string, object> param in parameters)
            {
                lines.Add($"{tab}{param.Key} = @{param.Key}");
                attributes["@" + param.Key] = param.Value ?? DBNull.Value;
            }

            stringBuilder.Append(string.Join(" and \n", lines));
            stringBuilder.AppendLine(";");

            return (stringBuilder.ToString(), attributes);
        }
    }
}
