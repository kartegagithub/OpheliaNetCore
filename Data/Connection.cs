using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Common;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;

namespace Ophelia.Data
{
    public class Connection : DbConnection
    {
        public QueryLogger Logger { get; set; }
        private DbConnection internalConnection;
        private DatabaseType _Type;
        private DataContext Context;
        public DatabaseType Type
        {
            get
            {
                return this._Type;
            }
            internal set
            {
                this._Type = value;
            }
        }
        public bool CloseAfterExecution { get; set; }

        public override string ConnectionString
        {
            get
            {
                return this.internalConnection.ConnectionString;
            }

            set
            {
                this.internalConnection.ConnectionString = value;
            }
        }

        public override string Database
        {
            get
            {
                return this.internalConnection.Database;
            }
        }

        public override string DataSource
        {
            get
            {
                return this.internalConnection.DataSource;
            }
        }

        public override string ServerVersion
        {
            get
            {
                return this.internalConnection.ServerVersion;
            }
        }

        public override ConnectionState State
        {
            get
            {
                return this.internalConnection.State;
            }
        }

        public override void ChangeDatabase(string databaseName)
        {
            this.internalConnection.ChangeDatabase(databaseName);
        }

        public override void Close()
        {
            this.internalConnection.Close();
        }

        public override void Open()
        {
            this.internalConnection.Open();
        }

        protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
        {
            return this.internalConnection.BeginTransaction(isolationLevel);
        }

        protected override DbCommand CreateDbCommand()
        {
            var command = System.Data.Common.DbProviderFactories.GetFactory(this.internalConnection).CreateCommand();
            command.Connection = this.internalConnection;
            return command;
            //switch (this.Type)
            //{
            //    case DatabaseType.SQLServer:
            //        return new SqlCommand("", (SqlConnection)this.internalConnection);
            //    case DatabaseType.PostgreSQL:
            //        return new Npgsql.NpgsqlCommand("", (Npgsql.NpgsqlConnection)this.internalConnection);
            //    case DatabaseType.Oracle:
            //        return new Oracle.ManagedDataAccess.Client.OracleCommand("", (Oracle.ManagedDataAccess.Client.OracleConnection)this.internalConnection);
            //    case DatabaseType.MySQL:
            //        break;
            //    default:
            //        break;
            //}
            //return null;
        }

        protected virtual DbDataAdapter CreateDataAdapter()
        {
            return System.Data.Common.DbProviderFactories.GetFactory(this.internalConnection).CreateDataAdapter();
        }

        public virtual DbParameter CreateParameter(DbCommand cmd, string name, object value)
        {
            var param = cmd.CreateParameter();
            param.ParameterName = name;
            param.Value = value ?? DBNull.Value;
            return param;
        }

        public virtual DbParameter CreateParameter(DbCommand cmd, string name, object value, DbType type)
        {
            var param = cmd.CreateParameter();
            param.ParameterName = name;
            param.Value = value ?? DBNull.Value;
            param.DbType = type;
            return param;
        }

        public Connection(DataContext context, DatabaseType type, string ConnectionString)
        {
            this.Context = context;
            this.Logger = context.CreateLogger();
            this._Type = type;
            switch (this.Type)
            {
                case DatabaseType.SQLServer:
                    this.internalConnection = new SqlConnection(ConnectionString);
                    break;
                case DatabaseType.PostgreSQL:
                    this.internalConnection = new Npgsql.NpgsqlConnection(ConnectionString);
                    break;
                case DatabaseType.Oracle:
                    this.internalConnection = new Oracle.ManagedDataAccess.Client.OracleConnection(ConnectionString);
                    this.Context.Configuration.UseNamespaceAsSchema = false;
                    break;
                case DatabaseType.MySQL:
                    this.Context.Configuration.UseNamespaceAsSchema = false;
                    break;
            }

            this.CloseAfterExecution = false;
        }
        private string PreventOracleSemiColonError(string sql)
        {
            if (this.Type == DatabaseType.Oracle && sql.EndsWith(";"))
                sql = sql.Trim(';');
            return sql;
        }
        public string FormatSQL(string sql)
        {
            sql = sql.Replace("[", this.GetOpeningBracket()).Replace("]", this.GetClosingBracket()).Replace("@p", this.GetParameterNameSign() + "p");
            return this.PreventOracleSemiColonError(sql);
        }
        public object ExecuteNonQuery(string sql)
        {
            return this.ExecuteNonQuery(sql, null);
        }

        public object ExecuteNonQuery(string sql, params object[] parameters)
        {
            using (DbCommand cmd = this.CreateCommand())
            {
                var param = new List<DbParameter>();
                if (parameters != null)
                {
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        param.Add(this.CreateParameter(cmd, this.FormatParameterName("p" + i), parameters[i]));
                    }
                }
                return ExecuteNonQuery(cmd, sql, param.ToArray());
            }
        }

        public object ExecuteNonQuery(DbCommand cmd, string sql, DbParameter[] sqlParameters)
        {
            Model.SQLLog log = null;
            try
            {
                if (this.Context.Configuration.LogSQL)
                {
                    log = new Model.SQLLog(sql, sqlParameters);
                    this.Logger.LogSQL(log);
                    log.Start();
                }

                cmd.CommandText = this.PreventOracleSemiColonError(sql);
                if (sqlParameters != null)
                {
                    foreach (var param in sqlParameters)
                        cmd.Parameters.Add(param);
                }
                this.CheckConnection();
                return cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + " " + sql, ex);
            }
            finally
            {
                if (this.Context.Configuration.LogSQL)
                    log.Finish();

                if (this.CloseAfterExecution)
                    this.Close();
            }
        }

        public object ExecuteScalar(string sql)
        {
            return this.ExecuteScalar(sql, null);
        }

        public object ExecuteScalar(DbCommand cmd, string sql, DbParameter[] sqlParameters)
        {
            Model.SQLLog log = null;
            try
            {
                if (this.Context.Configuration.LogSQL)
                {
                    log = new Model.SQLLog(sql, sqlParameters);
                    this.Logger.LogSQL(log);
                    log.Start();
                }
                cmd.CommandText = this.PreventOracleSemiColonError(sql);
                if (sqlParameters != null)
                {
                    foreach (var param in sqlParameters)
                        cmd.Parameters.Add(param);
                }
                this.CheckConnection();
                return cmd.ExecuteScalar();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + " " + sql, ex);
            }
            finally
            {
                if (this.Context.Configuration.LogSQL)
                    log.Finish();
                if (this.CloseAfterExecution)
                    this.Close();
            }
        }

        public object ExecuteScalar(string sql, params object[] parameters)
        {
            using (var cmd = this.CreateCommand())
            {
                var param = new List<DbParameter>();
                if (parameters != null)
                {
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        param.Add(this.CreateParameter(cmd, this.FormatParameterName("p" + i), parameters[i]));
                    }
                }
                return ExecuteScalar(cmd, sql, param.ToArray());
            }
        }

        public DataTable GetData(string sqlSelect)
        {
            return this.GetData(sqlSelect, null);
        }

        public DataTable GetData(string sqlSelect, params object[] parameters)
        {
            return this.GetData(sqlSelect, 0, 0, parameters);
        }

        public DataTable GetData(string sqlSelect, int startRecord, int maxCount, params object[] parameters)
        {
            using (var cmd = this.CreateCommand())
            {
                var param = new List<DbParameter>();
                if (parameters != null)
                {
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        param.Add(this.CreateParameter(cmd, this.FormatParameterName("p" + i), parameters[i]));
                    }
                }
                return this.GetData(cmd, sqlSelect, startRecord, maxCount, param.ToArray());
            }
        }
        public DataTable GetPagedData(string sqlSelect, int page, int pageSize, params object[] parameters)
        {
            using (var cmd = this.CreateCommand())
            {
                var param = new List<DbParameter>();
                if (parameters != null)
                {
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        param.Add(this.CreateParameter(cmd, this.FormatParameterName("p" + i), parameters[i]));
                    }
                }
                return this.GetData(cmd, sqlSelect, (page - 1) * pageSize, pageSize, param.ToArray());
            }
        }
        public DataTable GetPagedData(DbCommand cmd, string sqlSelect, int page, int pageSize, DbParameter[] sqlParameters)
        {
            return this.GetData(cmd, sqlSelect, (page - 1) * pageSize, pageSize, sqlParameters);
        }
        public DataTable GetData(DbCommand cmd, string sqlSelect, int startRecord, int maxCount, DbParameter[] sqlParameters)
        {
            Model.SQLLog log = null;
            try
            {
                bool canApplyDBLevelPaging = maxCount > 0 && this.Type != DatabaseType.Oracle;
                if (canApplyDBLevelPaging)
                {
                    canApplyDBLevelPaging = sqlSelect.IndexOf(" TOP ", StringComparison.InvariantCultureIgnoreCase) == -1 &&
                        sqlSelect.IndexOf(" ORDER BY ", StringComparison.InvariantCultureIgnoreCase) > -1 &&
                        sqlSelect.IndexOf("ROWS FETCH NEXT", StringComparison.InvariantCultureIgnoreCase) == -1;
                    if (canApplyDBLevelPaging)
                        sqlSelect += " OFFSET " + startRecord + " ROWS FETCH NEXT " + maxCount + " ROWS ONLY";
                }

                if (this.Context.Configuration.LogSQL)
                {
                    log = new Model.SQLLog(sqlSelect, sqlParameters);
                    this.Logger.LogSQL(log);
                    log.Start();
                }
                cmd.CommandText = this.PreventOracleSemiColonError(sqlSelect);
                if (sqlParameters != null)
                {
                    foreach (var param in sqlParameters)
                        cmd.Parameters.Add(param);
                }

                var Adapter = this.CreateDataAdapter();
                Adapter.SelectCommand = cmd;
                DataSet DataSet = new System.Data.DataSet();

                this.CheckConnection();
                if (canApplyDBLevelPaging || maxCount == 0)
                    Adapter.Fill(DataSet, "Table1");
                else
                    Adapter.Fill(DataSet, startRecord, maxCount, "Table1");

                return DataSet.Tables[0];
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + " " + sqlSelect, ex);
            }
            finally
            {
                if (this.Context.Configuration.LogSQL)
                    log.Finish();
                if (this.CloseAfterExecution)
                    this.Close();
            }
        }
        public object FormatParameterValue(object value, bool isString = false)
        {
            if (value != null & !isString && (value.ToString() == "True" || value.ToString() == "False"))
            {
                return (value.ToString() == "True" ? 1 : 0);
            }
            return value;
        }
        public string FormatParameterName(string Name)
        {
            return this.GetParameterNameSign() + Name;
        }
        private string GetParameterNameSign()
        {
            switch (this.Type)
            {
                case DatabaseType.Oracle:
                    return ":";
                default:
                    return "@";
            }
        }
        public string FormatStringConcat(string Name)
        {
            switch (this.Type)
            {
                case DatabaseType.SQLServer:
                    return Name;
                case DatabaseType.PostgreSQL:
                    return Name.Replace("+", "||");
                case DatabaseType.Oracle:
                    return Name.Replace("+", "||");
                case DatabaseType.MySQL:
                    return Name;
            }
            return "";
        }

        private void CheckConnection()
        {
            if (this.State == ConnectionState.Closed)
            {
                this.Open();
                if (this.Type == DatabaseType.Oracle)
                {
                    var cae = this.CloseAfterExecution;
                    this.CloseAfterExecution = false;
                    this.ExecuteNonQuery("ALTER SESSION SET NLS_SORT=BINARY_CI");
                    this.ExecuteNonQuery("ALTER SESSION SET NLS_COMP=LINGUISTIC");
                    this.CloseAfterExecution = cae;
                }
            }
        }

        public long GetSequenceNextVal(Type type)
        {
            var seqName = this.GetSequenceName(type);
            switch (this.Type)
            {
                case DatabaseType.PostgreSQL:
                    return Convert.ToInt64(this.ExecuteScalar("SELECT nextval('" + seqName + "')"));
                case DatabaseType.Oracle:
                    return Convert.ToInt64(this.ExecuteScalar("SELECT " + seqName + ".nextval FROM DUAL"));
            }
            return 0;
        }
        public string GetSequenceName(Type type)
        {
            var tableName = this.GetTableName(type, false);
            if (this.Context.Connection.Type == DatabaseType.Oracle)
            {
                if (tableName.Length > 28)
                    return "S_" + tableName.Left(28);
                else
                    return "S_" + tableName;
            }
            return "SEQ_" + tableName;
        }
        public string GetTableName(string schema, string name, bool format = true, string databaseName = "")
        {
            var sb = new StringBuilder();
            if (format)
            {
                if (!string.IsNullOrEmpty(databaseName))
                    sb.Append(this.FormatDataElement(databaseName) + ".");

                if (this.Context.Configuration.UseNamespaceAsSchema)
                {
                    sb.Append(this.FormatDataElement(this.GetMappedNamespace(schema).Replace(".", "_"))).Append(".").Append(this.FormatDataElement(this.GetMappedTableName(name)));
                }
                else
                {
                    sb.Append(this.FormatDataElement(this.GetMappedNamespace(schema).Replace(".", "_") + "_" + this.GetMappedTableName(name)));
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(databaseName))
                    sb.Append(this.FormatDataElement(databaseName) + ".");
                sb.Append(this.GetMappedNamespace(schema).Replace(".", "_")).Append("_").Append(this.GetMappedTableName(name));
            }
            return sb.ToString();
        }
        internal string GetMappedNamespace(string sp)
        {
            if (this.Context.NamespaceMap.ContainsKey(sp))
            {
                sp = this.Context.NamespaceMap[sp];
            }
            if (this.Type == DatabaseType.Oracle)
                sp = sp.ToUpper().Replace("İ", "I");
            return sp;
        }
        internal string GetMappedTableName(string tb)
        {
            if (this.Context.TableMap.ContainsKey(tb))
            {
                tb = this.Context.TableMap[tb];
            }
            if (this.Type == DatabaseType.Oracle)
                tb = tb.ToUpper().Replace("İ", "I");
            return tb;
        }
        internal string GetMappedFieldName(string f)
        {
            if (this.Context.FieldMap.ContainsKey(f))
            {
                f = this.Context.FieldMap[f];
            }
            if (this.Type == DatabaseType.Oracle)
                f = f.ToUpper().Replace("İ", "I");
            return f;
        }
        public string GetTableName(Type type, bool format = true)
        {
            var dbName = "";
            if (this.Context.Configuration.AllowLinkedDatabases && !this.Context.ContainsEntityType(type))
            {
                var ctx = this.Context.GetContext(type);
                if (ctx != null && ctx.Connection.Database != this.Context.Connection.Database)
                    dbName = ctx.Connection.Database;
            }

            return GetTableName(this.GetSchema(type), type.Name, format, dbName);
        }
        public string GetSchema(Type type)
        {
            var schema = type.Namespace;
            foreach (var key in this.Context.Configuration.NamespacesToIgnore)
            {
                schema = schema.Replace(key, "").Trim('.');
            }
            return this.GetMappedNamespace(schema);
        }
        public string GetPrimaryKeyName(Type type)
        {
            if (this.Context.Configuration.PrimaryKeyContainsEntityName)
                return this.FormatDataElement(this.GetMappedFieldName(type.Name + "ID"));
            else
                return this.FormatDataElement(this.GetMappedFieldName("ID"));
        }

        internal string FormatDataElement(string key)
        {
            return this.GetOpeningBracket() + this.CheckCharLimit(key) + this.GetClosingBracket();
        }
        internal string CheckCharLimit(string key)
        {
            if (this.Type == DatabaseType.Oracle && key.Length > 30)
            {
                key = key.Left(30);
            }
            return key;
        }
        private string GetOpeningBracket()
        {
            switch (this.Type)
            {
                case DatabaseType.SQLServer:
                    return "[";
                case DatabaseType.PostgreSQL:
                    return "\"";
                case DatabaseType.Oracle:
                    return "\"";
                case DatabaseType.MySQL:
                    return "'";
            }
            return "";
        }
        private string GetClosingBracket()
        {
            switch (this.Type)
            {
                case DatabaseType.SQLServer:
                    return "]";
                case DatabaseType.PostgreSQL:
                    return "\"";
                case DatabaseType.Oracle:
                    return "\"";
                case DatabaseType.MySQL:
                    return "'";
            }
            return "";
        }
        public PropertyInfo[] GetAllFields(Querying.Query.Helpers.Table table)
        {
            return table.EntityType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        }
        public string GetAllSelectFields(Querying.Query.Helpers.Table table, bool isSubTable = true, bool loadByXML = false)
        {
            var sb = new StringBuilder();
            var excludedProps = table.EntityType.GetCustomAttributes(typeof(Attributes.ExcludeDefaultColumn));
            var properties = this.GetAllFields(table);
            foreach (var p in properties)
            {
                if (table.Query.Data.Excluders.Where(op => op.Name == p.Name).Any())
                {
                    continue;
                }
                else
                {
                    if (p.PropertyType.Name != "String" && typeof(System.Collections.IEnumerable).IsAssignableFrom(p.PropertyType))
                        continue;
                    if (p.PropertyType.Name != "String" && p.PropertyType.IsClass && !p.PropertyType.IsValueType)
                        continue;
                    if (excludedProps == null || !excludedProps.Where(op => ((Attributes.ExcludeDefaultColumn)op).Columns.Contains(p.Name)).Any())
                    {
                        var pAttributes = p.GetCustomAttributes(typeof(System.ComponentModel.DataAnnotations.Schema.NotMappedAttribute));
                        if (pAttributes == null || pAttributes.Count == 0)
                        {
                            var fieldStr = this.GetFieldSelectString(table, p, isSubTable, loadByXML);
                            sb.Append(fieldStr);
                            if (!string.IsNullOrEmpty(fieldStr))
                                sb.Append(",");
                        }
                    }
                }
            }

            return sb.ToString().Trim(',');
        }

        public string GetFieldSelectString(Querying.Query.Helpers.Table table, PropertyInfo p, bool isSubTable = true, bool loadByXML = false)
        {
            var sb = new StringBuilder();

            if (!p.CanWrite || !p.CanRead) { return ""; }

            MethodInfo mget = p.GetGetMethod(false);
            MethodInfo mset = p.GetSetMethod(false);

            // Get and set methods have to be public
            if (mget == null) { return ""; }
            if (mset == null) { return ""; }

            if (p.PropertyType.IsDataEntity()) { return ""; }
            if (p.PropertyType.IsQueryableDataSet()) { return ""; }

            var alias = this.FormatDataElement(this.GetMappedFieldName(table.Alias + "_" + p.Name));
            if (isSubTable && loadByXML)
            {
                if (table.Query.Context.Connection.Type == DatabaseType.PostgreSQL)
                {
                    sb.Append("XMLELEMENT(name ");
                    sb.Append(alias);
                    sb.Append(", ");
                }
                else if (table.Query.Context.Connection.Type == DatabaseType.Oracle)
                {
                    sb.Append("XMLELEMENT(");
                    sb.Append(alias);
                    sb.Append(", ");
                }
            }
            sb.Append(table.Alias);
            sb.Append(".");
            sb.Append(this.FormatDataElement(this.GetMappedFieldName(p.Name)));
            if (isSubTable && table.Query.Context.Connection.Type == DatabaseType.SQLServer)
            {
                sb.Append(" AS ");
                sb.Append(alias);
            }
            if (isSubTable && loadByXML && (table.Query.Context.Connection.Type == DatabaseType.PostgreSQL || table.Query.Context.Connection.Type == DatabaseType.Oracle))
            {
                sb.Append(")");
            }
            else if (isSubTable && !loadByXML && (table.Query.Context.Connection.Type == DatabaseType.PostgreSQL || table.Query.Context.Connection.Type == DatabaseType.Oracle))
            {
                sb.Append(" AS ");
                sb.Append(alias);
            }

            return sb.ToString();
        }

        protected override void Dispose(bool disposing)
        {
            this.Logger.Dispose();
            this.Logger = null;
            base.Dispose(disposing);
            GC.SuppressFinalize(this);
        }
    }
}
