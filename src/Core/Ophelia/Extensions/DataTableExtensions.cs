using System;
using System.Collections.Generic;
using System.Data;

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
    }
}
