using DataLayer.Models;
using System.Collections;
using System.Data;
using System.Reflection;

namespace DataLayer
{
    public class Mapper
    {
        public static bool CheckAttribute<T>(PropertyInfo property)
        {
            return property.GetCustomAttributes().OfType<T>().Any();
        }

        public static bool CheckNullable(PropertyInfo property)
        {
            return new NullabilityInfoContext().Create(property).WriteState is NullabilityState.Nullable;
        }

        public static string GetPropertyName(PropertyInfo property)
        {
            var customName = property.GetCustomAttribute<DbColumnNameAttribute>()?.Name;
            if (!string.IsNullOrEmpty(customName))
                return customName;
            else
                return property.Name;
        }

        public static string GetPropertySqlType(PropertyInfo property)
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
            else if (property.PropertyType == typeof(double) || property.PropertyType == typeof(double?))
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

        public static string GetSqlCreateTableParams(PropertyInfo property)
        {
            List<string> sqlParams = new List<string>
            {
                GetPropertyName(property),
                GetPropertySqlType(property)
            };

            if (CheckAttribute<DbPrimaryKeyAttribute>(property))
                sqlParams.Add("not null");
            else if (!CheckNullable(property))
                sqlParams.Add("not null");
            else
                sqlParams.Add("null");

            return String.Join(' ', sqlParams);
        }
        
        public static long? GetPrimaryIdValue<T>(T entry)
        {
            Type type =  typeof(T);
            
            var idList = type.GetProperties().Where(x => CheckAttribute<DbPrimaryKeyAttribute>(x)).ToArray();
            if (idList.Length != 1)
                throw new Exception($"Invalid number of primary keys: {idList.Length}");
            
            var idProperty = idList.First();
            var idValue = idProperty.GetValue(entry);
            
            if (idValue != null)
                return Convert.ToInt64(idValue);
            return null;
        }
        public static string GetPrimaryIdName<T>()
        {
            Type type = typeof(T);
            var idList = type.GetProperties().Where(x => CheckAttribute<DbPrimaryKeyAttribute>(x)).ToArray();
            if (idList.Length != 1)
                throw new Exception($"Invalid number of primary keys: {idList.Length}");
            var idProperty = idList[0];
            return GetPropertyName(idProperty);
        }
        public static object? GetPropertyValue<T>(T entry, PropertyInfo property)
        {
            var value = property.GetValue(entry);

            if (value == DBNull.Value )
                return null;
            if (CheckAttribute<DbForeignKeyAttribute>(property))
            {
                MethodInfo method = typeof(Mapper).GetMethods().Where(m => m.Name == "GetPrimaryIdValue" && m.GetParameters().Length == 1).First();
                MethodInfo generic = method.MakeGenericMethod(property.PropertyType);
                var idPropertyName = generic.Invoke(null, new object[] { value! })!;
                return idPropertyName;
            }
            return value;
        }
        
        public static void SetPrimaryIdValue<T>(T entry, long? value)
        {
            Type type = typeof(T);
            var idList = type.GetProperties().Where(x => CheckAttribute<DbPrimaryKeyAttribute>(x)).ToArray();
            if (idList.Length != 1)
                throw new Exception($"Invalid number of primary keys: {idList.Length}");
            var idProperty = idList[0];
            idProperty.SetValue(entry, value);
        }
        public static void SetPropertyValue<T>(T entry, string propertyName, object? value)
        {
            Type type = typeof(T);
            var property = type.GetProperty(propertyName);
            if (property == null)
                throw new Exception($"Property {propertyName} not found");
            property.SetValue(entry, value);
        }
        public static void SetPropertyValue<T>(T entry, PropertyInfo property, object? value)
        {
            property.SetValue(entry, value);
        }
        
        public static object? ConvertFromSqlType(object? value, PropertyInfo property)
        {
            if (value == DBNull.Value)
                return null;
            else if (CheckNullable(property) && property.PropertyType != typeof(string))
                return Convert.ChangeType(value, Nullable.GetUnderlyingType(property.PropertyType)!);
            else
                return Convert.ChangeType(value, property.PropertyType);
        }
        public static T MapRowToT<T>(DataRow row)
        {
            T entry = Activator.CreateInstance<T>();
            var properties = typeof(T).GetProperties();
            
            foreach (var property in properties)
            {
                object? value = row[GetPropertyName(property)];
                
                if (CheckAttribute<DbForeignKeyAttribute>(property))
                {
                    MethodInfo method = typeof(Database).GetMethods().Where(m => m.Name == "Select" && m.GetParameters().Length == 1).First();
                    MethodInfo generic = method.MakeGenericMethod(property.PropertyType);
                    var foreignItem = ((IEnumerable)generic.Invoke(null, new object[] { Convert.ToInt64(value) })!).Cast<object>().ToList();
                    
                    if (foreignItem.Count == 0)
                        throw new Exception($"Foreign key {property.Name} not found");
                    else if (foreignItem.Count > 1)
                        throw new Exception($"Foreign key {property.Name} not unique");
                    else
                        value = foreignItem[0];
                }
                else
                {
                    value = ConvertFromSqlType(value, property);
                }

                SetPropertyValue(entry, property, value);
            }

            return entry;
        }
    }
}
