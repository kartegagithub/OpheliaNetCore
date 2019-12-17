using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Diagnostics;
using System.ComponentModel.DataAnnotations.Schema;
namespace Ophelia.Data.Querying
{
    public class DataDesigner : IDisposable
    {
        private Query.BaseQuery Query { get; set; }
        private DataContext Context { get; set; }
        private List<string> SQLList;
        public bool Check(Query.QueryBuilder query, Exception ex)
        {
            this.Context = query.Context;
            if (this.Context.Configuration.AllowStructureAutoCreation && this.IsValidException(ex))
            {
                this.SQLList = new List<string>();
                foreach (var table in query.Tables)
                {
                    this.CreateTable(table);
                }
                foreach (var sql in this.SQLList)
                {
                    try
                    {
                        this.Context.Connection.ExecuteNonQuery(sql);
                    }
                    catch (Exception ex2)
                    {
                        Debug.WriteLine("Designer:1:" + ex2.Message);
                    }
                }
                return true;
            }
            return false;
        }
        public bool Check(Query.BaseQuery query, Exception ex)
        {
            this.Query = query;
            this.Context = query.Context;
            if (this.Context.Configuration.AllowStructureAutoCreation && this.IsValidException(ex))
            {
                this.SQLList = new List<string>();
                this.CreateTables();
                foreach (var sql in this.SQLList)
                {
                    try
                    {
                        this.Context.Connection.ExecuteNonQuery(sql);
                    }
                    catch (Exception ex2)
                    {
                        Debug.WriteLine("Designer:1:" + ex2.Message);
                    }
                }
                return true;
            }
            return false;
        }

        private bool IsValidException(Exception ex)
        {
            if (ex.Message.IndexOf("No value given for one or more required parameters", StringComparison.InvariantCultureIgnoreCase) > -1
                || ex.Message.IndexOf("does not belong to table", StringComparison.InvariantCultureIgnoreCase) > -1
                || ex.Message.IndexOf("Invalid object name", StringComparison.InvariantCultureIgnoreCase) > -1
                || ex.Message.IndexOf("Could not find output table", StringComparison.InvariantCultureIgnoreCase) > -1
                || ex.Message.IndexOf("unknown field name", StringComparison.InvariantCultureIgnoreCase) > -1
                || ex.Message.IndexOf("ORA-02289", StringComparison.InvariantCultureIgnoreCase) > -1
                || ex.Message.IndexOf("ORA-00942", StringComparison.InvariantCultureIgnoreCase) > -1
                || ex.Message.IndexOf("ORA-00904", StringComparison.InvariantCultureIgnoreCase) > -1
                || ex.Message.IndexOf("ORA-00001", StringComparison.InvariantCultureIgnoreCase) > -1
                || ex.Message.IndexOf("Invalid column name", StringComparison.InvariantCultureIgnoreCase) > -1
                || ex.Message.IndexOf("42703", StringComparison.InvariantCultureIgnoreCase) > -1
                || ex.Message.IndexOf("42P01", StringComparison.InvariantCultureIgnoreCase) > -1
                || ex.Message.IndexOf("42704", StringComparison.InvariantCultureIgnoreCase) > -1)
            {
                return true;
            }
            return false;
        }

        private void CreateSchema(Type type)
        {
            if (this.Context.Configuration.UseNamespaceAsSchema)
            {
                var dbName = "";
                if (this.Context.Configuration.AllowLinkedDatabases && !this.Context.ContainsEntityType(type))
                {
                    var ctx = this.Context.GetContext(type);
                    if (ctx != null && ctx.Connection.Database != this.Context.Connection.Database)
                        dbName = ctx.Connection.Database + ".";
                }
                switch (this.Context.Connection.Type)
                {
                    case DatabaseType.SQLServer:
                        this.AddSQL("CREATE SCHEMA " + dbName + this.Context.Connection.GetSchema(type));
                        break;
                    case DatabaseType.PostgreSQL:
                        this.AddSQL("CREATE SCHEMA \"" + dbName + this.Context.Connection.GetSchema(type) + "\"");
                        break;
                }
            }
        }
        private void CreateTables(Query.Helpers.Table table = null)
        {
            if (this.Query.Data.MainTable == null)
                this.Query.Data.MainTable = new Query.Helpers.Table(this.Query, this.Query.Data.EntityType);
            if (table == null)
                table = this.Query.Data.MainTable;

            this.CreateSchema(table.EntityType);
            this.CreateTable(table);
            this.CheckFilter(this.Query.Data.Filter);
            if (this.Query.Data.Includers.Count > 0)
            {
                foreach (var item in this.Query.Data.Includers)
                {
                    this.CheckIncluder(item);
                }
            }
        }
        private void CheckIncluder(Query.Helpers.Includer includer)
        {
            if (includer.PropertyInfo != null)
            {
                var n2nRelationAttribute = includer.PropertyInfo.GetCustomAttributes(typeof(Attributes.N2NRelationProperty)).FirstOrDefault() as Attributes.N2NRelationProperty;
                if (n2nRelationAttribute != null)
                    this.CreateTable(n2nRelationAttribute.RelationClassType);
            }
            this.CreateTable(includer.EntityType);
            foreach (var item in includer.SubIncluders)
            {
                this.CheckIncluder(item);
            }
        }
        private void CheckJoins(Query.Helpers.Table table)
        {
            if (table.Joins.Count > 0)
            {
                foreach (var item in table.Joins)
                {
                    this.CreateTable(item);
                }
            }
        }
        private void CheckFilter(Query.Helpers.Filter filter)
        {
            if (filter != null)
            {
                if (filter.EntityType != null)
                    this.CreateTable(filter.EntityType);
                if (filter.SubFilter != null)
                    this.CheckFilter(filter.SubFilter);
                if (filter.Right != null)
                    this.CheckFilter(filter.Right);
                if (filter.Left != null)
                    this.CheckFilter(filter.Left);
            }
        }
        private void AddSQL(string SQL)
        {
            if (!this.SQLList.Contains(SQL))
                this.SQLList.Add(SQL);
        }
        private void CreateTable(Query.Helpers.Table table)
        {
            if (table.EntityType.IsDataEntity())
            {
                this.CreateTable(table.EntityType);
                this.CheckJoins(table);
            }
        }
        private void CreateTable(Type type)
        {
            this.CreateSchema(type);
            var pkey = this.Context.Connection.GetPrimaryKeyName(type);
            var table = this.Context.Connection.GetTableName(type);
            switch (this.Context.Connection.Type)
            {
                case DatabaseType.SQLServer:
                    this.AddSQL("CREATE TABLE " + table + " (" + pkey + " bigint Not Null IDENTITY Primary Key)");
                    break;
                case DatabaseType.PostgreSQL:
                    this.AddSQL("CREATE TABLE " + table + " (" + pkey + " BIGINT, PRIMARY KEY (" + pkey + "))");
                    break;
                case DatabaseType.Oracle:
                    this.AddSQL("CREATE TABLE " + table + " (" + pkey + " NUMBER(38), PRIMARY KEY (" + pkey + ") VALIDATE )");
                    break;
                case DatabaseType.MySQL:
                    this.AddSQL("CREATE TABLE " + table + " (" + pkey + " BIGINT NOT NULL AUTO_INCREMENT PRIMARY KEY)");
                    break;
            }
            this.CreateSequence(type);
            this.CreateFields(type);
        }
        private void CreateSequence(Type type)
        {
            var seqName = this.Context.Connection.GetSequenceName(type);
            switch (this.Context.Connection.Type)
            {
                case DatabaseType.PostgreSQL:
                    this.AddSQL("CREATE SEQUENCE " + seqName + " START 1");
                    break;
                case DatabaseType.Oracle:
                    this.AddSQL("CREATE SEQUENCE " + seqName + " START WITH 1 INCREMENT BY 1 NOCACHE NOCYCLE");
                    break;
            }
        }
        private void CreateFields(Type type)
        {
            var relationClassproperty = type.GetCustomAttributes(typeof(Attributes.RelationClass)).FirstOrDefault() as Attributes.RelationClass;

            var excludedProps = type.GetCustomAttributes(typeof(Attributes.ExcludeDefaultColumn));

            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(op => !op.PropertyType.IsDataEntity() && !op.PropertyType.IsQueryableDataSet());
            foreach (var p in properties)
            {
                var isNotMapped = p.GetCustomAttributes(typeof(NotMappedAttribute)).Count > 0;
                if (isNotMapped)
                    continue;

                if (excludedProps == null || !excludedProps.Where(op => ((Attributes.ExcludeDefaultColumn)op).Columns.Contains(p.Name)).Any())
                {
                    if (relationClassproperty == null || type.IsAssignableFrom(p.DeclaringType))
                        this.CreateField(type, p);
                }
            }
        }
        private void CreateField(Type tableType, PropertyInfo p)
        {
            Attributes.DataProperty DataAttribute = null;
            if (p.CustomAttributes.Count() > 0)
            {
                if (p.GetCustomAttributes(typeof(NotMappedAttribute)).Any())
                {
                    return;
                }
                DataAttribute = (Attributes.DataProperty)p.GetCustomAttributes(typeof(Attributes.DataProperty)).FirstOrDefault();
            }
            var datatype = this.GetSQLDataType(p.PropertyType, DataAttribute);
            var tableName = this.Context.Connection.GetTableName(tableType);
            var primaryKeyName = this.Context.Connection.GetPrimaryKeyName(tableType);
            var fieldName = this.Context.Connection.FormatDataElement(this.Context.Connection.GetMappedFieldName(p.Name));
            if (fieldName.Equals(primaryKeyName))
                return;

            var nullable = Nullable.GetUnderlyingType(p.PropertyType) != null;
            if (!nullable && p.PropertyType.IsAssignableFrom(typeof(string)))
                nullable = true;
            if (!nullable && p.PropertyType.IsAssignableFrom(typeof(DateTime)))
                nullable = true;

            if (!string.IsNullOrEmpty(datatype))
            {
                var collate = "";
                if (this.Context.Connection.Type == DatabaseType.Oracle && !string.IsNullOrEmpty(this.Context.Configuration.OracleStringColumnCollation) && !string.IsNullOrEmpty(this.Context.Configuration.DatabaseVersion) && this.Context.Configuration.DatabaseVersion.IndexOf("11") == -1)
                    collate = " COLLATE " + this.Context.Configuration.OracleStringColumnCollation;

                var defaultValue = this.GetDefaultValue(p.PropertyType);
                if (defaultValue != null)
                {
                    this.AddSQL("ALTER TABLE " + tableName + " ADD " + fieldName + " " + datatype + collate + " DEFAULT " + defaultValue + (!nullable ? " NOT NULL " : ""));
                }
                else
                    this.AddSQL("ALTER TABLE " + tableName + " ADD " + fieldName + " " + datatype + collate + (!nullable ? " NOT NULL" : ""));
            }
        }
        private string GetDefaultValue(Type type)
        {
            if (type == typeof(string))
            {
                return null;
            }
            else if (type == typeof(char) || type == typeof(Nullable<char>))
            {
                return null;
            }
            else if (type == typeof(byte) || type == typeof(Nullable<byte>))
            {
                return "0";
            }
            else if (type == typeof(bool) || type == typeof(Nullable<bool>))
            {
                return this.Context.Connection.FormatParameterValue(false).ToString();
            }
            else if (type == typeof(Int32) || type == typeof(int) || type == typeof(Int16) || type == typeof(Single) || type == typeof(Nullable<Int32>) || type == typeof(Nullable<int>) || type == typeof(Nullable<Int16>) || type == typeof(Nullable<Single>))
            {
                return "0";
            }
            else if (type == typeof(Int64) || type == typeof(long) || type == typeof(Nullable<Int64>) || type == typeof(Nullable<long>))
            {
                return "0";
            }
            else if (type == typeof(decimal) || type == typeof(float) || type == typeof(double) || type == typeof(Nullable<decimal>) || type == typeof(Nullable<float>) || type == typeof(Nullable<double>))
            {
                return "0";
            }
            else if (type == typeof(DateTime) || type == typeof(Nullable<DateTime>))
            {
                return null;
            }
            return null;
        }
        private string GetSQLDataType(Type type, Attributes.DataProperty dataAttribute)
        {
            if (type == typeof(string))
            {
                string lenght = this.Context.Configuration.DefaultStringColumnSize.ToString();
                if (dataAttribute != null)
                {
                    if (dataAttribute.Precision == int.MaxValue)
                    {
                        lenght = "MAX";
                    }
                    else
                        lenght = dataAttribute.Precision.ToString();
                }
                switch (this.Context.Connection.Type)
                {
                    case DatabaseType.SQLServer:
                        return "nvarchar(" + lenght + ")";
                    case DatabaseType.PostgreSQL:
                        if (lenght == "MAX")
                            return "text";
                        else
                            return "varchar(" + lenght + ")";
                    case DatabaseType.Oracle:
                        if (lenght == "MAX")
                            return "nclob";
                        else
                            return "varchar(" + lenght + ")";
                    case DatabaseType.MySQL:
                        if (lenght == "MAX")
                            return "LONGTEXT";
                        else
                            return "varchar(" + lenght + ")";
                }
            }
            else if (type == typeof(char) || type == typeof(Nullable<char>))
            {
                switch (this.Context.Connection.Type)
                {
                    case DatabaseType.SQLServer:
                        return "nchar(1)";
                    case DatabaseType.PostgreSQL:
                        return "char(1)";
                    case DatabaseType.Oracle:
                        return "CHAR(1)";
                    case DatabaseType.MySQL:
                        return "CHAR(1)";
                }
            }
            else if (type == typeof(byte) || type == typeof(Nullable<byte>))
            {
                switch (this.Context.Connection.Type)
                {
                    case DatabaseType.SQLServer:
                        return "tinyint";
                    case DatabaseType.PostgreSQL:
                        return "smallint";
                    case DatabaseType.Oracle:
                        return "NUMBER(3)";
                    case DatabaseType.MySQL:
                        return "TINYINT";
                }
            }
            else if (type == typeof(bool) || type == typeof(Nullable<bool>))
            {
                switch (this.Context.Connection.Type)
                {
                    case DatabaseType.SQLServer:
                        return "bit";
                    case DatabaseType.PostgreSQL:
                        return "smallint";
                    case DatabaseType.Oracle:
                        return "NUMBER(1)";
                    case DatabaseType.MySQL:
                        return "TINYINT";
                }
            }
            else if (type == typeof(Int32) || type == typeof(int) || type == typeof(Int16) || type == typeof(Single) || type == typeof(Nullable<Int32>) || type == typeof(Nullable<int>) || type == typeof(Nullable<Int16>) || type == typeof(Nullable<Single>))
            {
                switch (this.Context.Connection.Type)
                {
                    case DatabaseType.SQLServer:
                        return "int";
                    case DatabaseType.PostgreSQL:
                        return "int";
                    case DatabaseType.Oracle:
                        return "NUMBER";
                    case DatabaseType.MySQL:
                        return "INT";
                }
            }
            else if (type == typeof(Int64) || type == typeof(long) || type == typeof(Nullable<long>) || type == typeof(Nullable<Int64>))
            {
                switch (this.Context.Connection.Type)
                {
                    case DatabaseType.SQLServer:
                        return "bigint";
                    case DatabaseType.PostgreSQL:
                        return "BIGINT";
                    case DatabaseType.Oracle:
                        return "NUMBER(38)";
                    case DatabaseType.MySQL:
                        return "BIGINT";
                }
            }
            else if (type == typeof(decimal) || type == typeof(float) || type == typeof(double) || type == typeof(Nullable<decimal>) || type == typeof(Nullable<float>) || type == typeof(Nullable<double>))
            {
                int precision = this.Context.Configuration.DefaultDecimalColumnPrecision;
                int scale = this.Context.Configuration.DefaultDecimalColumnScale;
                if (dataAttribute != null)
                {
                    if (dataAttribute.Precision == int.MaxValue)
                        precision = 38;
                    else
                        precision = dataAttribute.Precision;
                    if (dataAttribute.Scale == int.MaxValue)
                        scale = 5;
                    else
                        scale = dataAttribute.Scale;
                }
                switch (this.Context.Connection.Type)
                {
                    case DatabaseType.SQLServer:
                        return "decimal(" + precision + "," + scale + ")";
                    case DatabaseType.PostgreSQL:
                        return "NUMERIC(" + precision + "," + scale + ")";
                    case DatabaseType.Oracle:
                        return "NUMBER(" + precision + "," + scale + ")";
                    case DatabaseType.MySQL:
                        return "decimal(" + precision + "," + scale + ")";
                }
            }
            else if (type == typeof(DateTime) || type == typeof(Nullable<DateTime>))
            {
                switch (this.Context.Connection.Type)
                {
                    case DatabaseType.SQLServer:
                        return "datetime2(7)";
                    case DatabaseType.PostgreSQL:
                        return "timestamp with time zone";
                    case DatabaseType.Oracle:
                        return "DATE";
                    case DatabaseType.MySQL:
                        return "datetime";
                }
            }
            else if (type == typeof(TimeSpan) || type == typeof(Nullable<TimeSpan>))
            {
                switch (this.Context.Connection.Type)
                {
                    case DatabaseType.SQLServer:
                        return "time";
                    case DatabaseType.PostgreSQL:
                        return "time without time zone";
                    case DatabaseType.Oracle:
                        return "DATE";
                    case DatabaseType.MySQL:
                        return "TIME";
                }
            }

            return "";
        }
        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
