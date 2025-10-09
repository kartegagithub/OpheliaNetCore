using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Reflection;

namespace Ophelia
{
    public static class DataTableExtensions
    {
        public static int ToInt32(this DataRow row, string ColumnName)
        {
            if (!string.IsNullOrEmpty(ColumnName) && row != null && row.Table.Columns.Contains(ColumnName) && row[ColumnName] != DBNull.Value)
            {
                return Convert.ToInt32(Convert.ToString(row[ColumnName]).Replace(" ", ""));
            }
            return 0;
        }

        public static long ToInt64(this DataRow row, string ColumnName)
        {
            if (!string.IsNullOrEmpty(ColumnName) && row != null && row.Table.Columns.Contains(ColumnName) && row[ColumnName] != DBNull.Value)
            {
                return Convert.ToInt64(Convert.ToString(row[ColumnName]).Replace(" ", ""));
            }
            return 0;
        }

        public static string ToString(this DataRow row, string ColumnName)
        {
            if (!string.IsNullOrEmpty(ColumnName) && row != null && row.Table.Columns.Contains(ColumnName) && row[ColumnName] != DBNull.Value)
            {
                return Convert.ToString(row[ColumnName]);
            }
            return "";
        }

        public static decimal ToDecimal(this DataRow row, string ColumnName)
        {
            if (!string.IsNullOrEmpty(ColumnName) && row != null && row.Table.Columns.Contains(ColumnName) && row[ColumnName] != DBNull.Value)
            {
                return Convert.ToDecimal(Convert.ToString(row[ColumnName]).Replace(" ", ""));
            }
            return 0;
        }
        public static decimal ToDecimal(this DataRow row, string ColumnName, string seperator, List<string> replaces = null)
        {
            if (!string.IsNullOrEmpty(ColumnName) && row != null && row.Table.Columns.Contains(ColumnName) && row[ColumnName] != DBNull.Value)
            {
                var rowValue = Convert.ToString(row[ColumnName]).Replace(" ", "");
                if (replaces != null && replaces.Count > 0)
                {
                    foreach (var item in replaces)
                    {
                        rowValue = rowValue.Replace(item, "");
                    }
                }

                if (seperator == ",")
                    return Convert.ToDecimal(rowValue.Replace(".", ","));
                else
                    return Convert.ToDecimal(rowValue.Replace(",", "."));
            }
            return 0;
        }
        public static DateTime ToDateTime(this DataRow row, string ColumnName)
        {
            if (!string.IsNullOrEmpty(ColumnName) && row != null && row.Table.Columns.Contains(ColumnName) && row[ColumnName] != DBNull.Value)
            {
                return Convert.ToDateTime(row[ColumnName]);
            }
            return DateTime.MinValue;
        }
        public static bool ToBoolean(this DataRow row, string ColumnName)
        {
            if (!string.IsNullOrEmpty(ColumnName) && row != null && row.Table.Columns.Contains(ColumnName) && row[ColumnName] != DBNull.Value)
            {
                return Convert.ToBoolean(row[ColumnName]);
            }
            return false;
        }
        public static byte ToByte(this DataRow row, string ColumnName)
        {
            if (!string.IsNullOrEmpty(ColumnName) && row != null && row.Table.Columns.Contains(ColumnName) && row[ColumnName] != DBNull.Value)
            {
                return Convert.ToByte(row[ColumnName]);
            }
            return 0;
        }

        public static List<T> ToList<T>(this DataTable table)
        {
            var list = new List<T>();
            var type = typeof(T);
            if (type == typeof(ExpandoObject) || type.Name == "Object")
            {
                foreach (DataRow row in table.Rows)
                {
                    dynamic dynamicObject = new ExpandoObject();
                    var expandoDict = (IDictionary<string, object>)dynamicObject;

                    foreach (DataColumn column in table.Columns)
                    {
                        expandoDict[column.ColumnName] = row[column] == DBNull.Value ? null : row[column];
                    }

                    list.Add((T)(object)dynamicObject);
                }
            }
            else
            {
                var columnMapping = new Dictionary<string, string>();
                foreach (DataColumn column in table.Columns)
                {
                    var columnName = column.ColumnName.ToLowerInvariant().Replace("ı", "i").Replace(" ", "_");
                    columnMapping[columnName] = column.ColumnName;
                }

                var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.CanWrite).ToList();
                var propertyMapping = new Dictionary<string, PropertyInfo>();
                foreach (var prop in properties)
                {
                    var propName = prop.Name.ToLowerInvariant().Replace("ı", "i").Replace(" ", "_");
                    propertyMapping[propName] = prop;
                }

                foreach (DataRow row in table.Rows)
                {
                    var obj = Activator.CreateInstance<T>();
                    foreach (var map in propertyMapping)
                    {
                        if (columnMapping.TryGetValue(map.Key, out string columnName) && !string.IsNullOrEmpty(columnName))
                        {
                            var value = row[columnName];
                            if (value != DBNull.Value)
                            {
                                try
                                {
                                    var propertyType = Nullable.GetUnderlyingType(map.Value.PropertyType) ?? map.Value.PropertyType;
                                    var convertedValue = propertyType.ConvertData(value);
                                    map.Value.SetValue(obj, convertedValue);
                                }
                                catch
                                {
                                    // Type dönüşümü başarısız olursa skip et
                                }
                            }
                        }
                    }

                    list.Add(obj);
                }
            }

            return list;
        }
    }
}
