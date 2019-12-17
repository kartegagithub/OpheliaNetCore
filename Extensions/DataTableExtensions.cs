using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace Ophelia
{
    public static class DataTableExtensions
    {
        public static int ToInt32(this DataRow row, string ColumnName)
        {
            if(!string.IsNullOrEmpty(ColumnName) && row != null && row.Table.Columns.Contains(ColumnName) && row[ColumnName] != DBNull.Value)
            {
                return Convert.ToInt32(row[ColumnName]);
            }
            return 0;
        }

        public static long ToInt64(this DataRow row, string ColumnName)
        {
            if (!string.IsNullOrEmpty(ColumnName) && row != null && row.Table.Columns.Contains(ColumnName) && row[ColumnName] != DBNull.Value)
            {
                return Convert.ToInt64(row[ColumnName]);
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
                return Convert.ToDecimal(row[ColumnName]);
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
        public static Boolean ToBoolean(this DataRow row, string ColumnName)
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
