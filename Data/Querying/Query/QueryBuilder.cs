using Ophelia.Data.Querying.Query.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ophelia.Data.Querying.Query
{
    public class QueryBuilder : IDisposable
    {
        private static QueryBuilder _Current;
        private QueryBuilder BaseQuery { get; set; }
        internal DataContext Context { get; set; }
        private StringBuilder StringBuilder { get; set; }
        private string LastTableName = "";
        private List<object> Parameters { get; set; }
        internal List<Type> Tables { get; set; }
        private bool DesignMode { get; set; }
        public static QueryBuilder Current
        {
            get
            {
                return _Current;
            }
        }
        public static QueryBuilder Init(DataContext context)
        {
            Reset();
            Current.Context = context;
            return Current;
        }
        public QueryBuilder AddComma()
        {
            this.StringBuilder.Append(", ");
            return this;
        }
        public QueryBuilder AddSum(string tableName, string fieldName, string alias = "")
        {
            if (string.IsNullOrEmpty(alias))
                alias = fieldName;
            this.StringBuilder.Append("SUM(" + this.FormatAndMapTable(tableName) + "." + this.FormatAndMapField(fieldName) + ") AS " + this.FormatAndMapField(alias));
            return this;
        }
        public QueryBuilder AddExists()
        {
            this.StringBuilder.Append(" EXISTS ");
            return this;
        }
        public QueryBuilder AddUnion()
        {
            this.StringBuilder.Append(" UNION ");
            return this;
        }
        public QueryBuilder AddSelect()
        {
            this.StringBuilder.Append("SELECT ");
            return this;
        }
        public QueryBuilder AddFrom()
        {
            this.StringBuilder.Append(" FROM ");
            return this;
        }
        public QueryBuilder AddOrderBy()
        {
            this.StringBuilder.Append(" ORDER BY ");
            return this;
        }
        public QueryBuilder AddOrderBy(string fieldName, string direction)
        {
            this.StringBuilder.Append(this.FormatAndMapField(fieldName) + " " + direction);
            return this;
        }
        public QueryBuilder AddOrderBy(string tableAlias, string fieldName, string direction)
        {
            this.StringBuilder.Append(this.FormatAndMapField(tableAlias) + "." + this.FormatAndMapField(fieldName) + " " + direction);
            return this;
        }
        public QueryBuilder AddGroupBy()
        {
            this.StringBuilder.Append(" GROUP BY ");
            return this;
        }
        public QueryBuilder AddHaving()
        {
            this.StringBuilder.Append(" HAVING ");
            return this;
        }
        public QueryBuilder AddWhere()
        {
            this.StringBuilder.Append(" WHERE ");
            return this;
        }
        public QueryBuilder AddAnd()
        {
            this.StringBuilder.Append(" AND ");
            return this;
        }
        public QueryBuilder AddOr()
        {
            this.StringBuilder.Append(" AND ");
            return this;
        }
        public QueryBuilder AddLeftParanthesis()
        {
            this.StringBuilder.Append("(");
            return this;
        }
        public QueryBuilder AddRightParanthesis()
        {
            this.StringBuilder.Append(")");
            return this;
        }
        public QueryBuilder AddEqualSign()
        {
            this.StringBuilder.Append(" = ");
            return this;
        }
        public QueryBuilder AddNotEqualSign()
        {
            if (this.Context.Connection.Type == DatabaseType.Oracle)
                this.StringBuilder.Append(" != ");
            else
                this.StringBuilder.Append(" <> ");
            return this;
        }
        public QueryBuilder AddStatement(string statement)
        {
            this.StringBuilder.Append(statement);
            return this;
        }
        public QueryBuilder AddParameter(string paramName)
        {
            this.StringBuilder.Append(this.Context.Connection.FormatParameterName(paramName));
            return this;
        }
        public QueryBuilder AddParameter(string paramName, object value)
        {
            this.StringBuilder.Append(this.Context.Connection.FormatParameterName(paramName));
            this.AddParamValue(value);
            return this;
        }
        public QueryBuilder FilterByField(string tableAlias, string fieldName, Comparison comparison, string toTableAlias, string toTableField)
        {
            var likeCmd = " LIKE ";
            if (this.Context.Connection.Type == DatabaseType.PostgreSQL)
                likeCmd = " ILIKE ";

            this.StringBuilder.Append(this.FormatAndMapTable(tableAlias)).Append(".").Append(FormatAndMapField(fieldName));
            switch (comparison)
            {
                case Comparison.Equal:
                    this.AddEqualSign();
                    this.AddStatement(this.FormatAndMapTable(toTableAlias) + "." + this.FormatAndMapField(toTableField));
                    break;
                case Comparison.Different:
                    this.AddNotEqualSign();
                    this.AddStatement(this.FormatAndMapTable(toTableAlias) + "." + this.FormatAndMapField(toTableField));
                    break;
                case Comparison.Greater:
                    this.AddStatement(" > ");
                    this.AddStatement(this.FormatAndMapTable(toTableAlias) + "." + this.FormatAndMapField(toTableField));
                    break;
                case Comparison.Less:
                    this.AddStatement(" < ");
                    this.AddStatement(this.FormatAndMapTable(toTableAlias) + "." + this.FormatAndMapField(toTableField));
                    break;
                case Comparison.GreaterAndEqual:
                    this.AddStatement(" >= ");
                    this.AddStatement(this.FormatAndMapTable(toTableAlias) + "." + this.FormatAndMapField(toTableField));
                    break;
                case Comparison.LessAndEqual:
                    this.AddStatement(" <= ");
                    this.AddStatement(this.FormatAndMapTable(toTableAlias) + "." + this.FormatAndMapField(toTableField));
                    break;
                case Comparison.In:
                    throw new NotImplementedException();

                case Comparison.StartsWith:
                    this.AddStatement(likeCmd);
                    this.AddStatement(this.Context.Connection.FormatStringConcat(" '%' + "));
                    this.AddStatement(this.FormatAndMapTable(toTableAlias) + "." + this.FormatAndMapField(toTableField));
                    break;
                case Comparison.EndsWith:
                    this.AddStatement(likeCmd);
                    this.AddStatement(this.FormatAndMapTable(toTableAlias) + "." + this.FormatAndMapField(toTableField));
                    this.AddStatement(this.Context.Connection.FormatStringConcat("+ '%'"));
                    break;
                case Comparison.Contains:
                    this.AddStatement(likeCmd);
                    this.AddStatement(this.Context.Connection.FormatStringConcat(" '%' + "));
                    this.AddStatement(this.FormatAndMapTable(toTableAlias) + "." + this.FormatAndMapField(toTableField));
                    this.AddStatement(this.Context.Connection.FormatStringConcat(" + '%'"));
                    break;
            }
            return this;
        }
        public QueryBuilder FilterByValue(string tableAlias, string fieldName, Comparison comparison, string value)
        {
            var likeCmd = "LIKE";
            if (this.Context.Connection.Type == DatabaseType.PostgreSQL)
                likeCmd = "ILIKE";
            if (!string.IsNullOrEmpty(value) && !value.IsNumeric())
            {
                value = value.Replace("'", "''");
                if (this.Context.Connection.Type == DatabaseType.Oracle)
                    value = value.ToUpper();
            }
            else if (string.IsNullOrEmpty(value))
                value = "''";

            this.StringBuilder.Append(this.FormatAndMapTable(tableAlias)).Append(".").Append(FormatAndMapField(fieldName));
            switch (comparison)
            {
                case Comparison.Equal:
                    this.AddEqualSign();
                    this.AddStatement(value);
                    break;
                case Comparison.Different:
                    this.AddNotEqualSign();
                    this.AddStatement(value);
                    break;
                case Comparison.Greater:
                    this.AddStatement(" > " + value);
                    break;
                case Comparison.Less:
                    this.AddStatement(" < " + value);
                    break;
                case Comparison.GreaterAndEqual:
                    this.AddStatement(" >= " + value);
                    break;
                case Comparison.LessAndEqual:
                    this.AddStatement(" <= " + value);
                    break;
                case Comparison.In:
                    this.AddStatement(" IN (" + value + ")");
                    break;
                case Comparison.StartsWith:
                    this.AddStatement(this.Context.Connection.FormatStringConcat(likeCmd + " '%" + value + "'"));
                    break;
                case Comparison.EndsWith:
                    this.AddStatement(this.Context.Connection.FormatStringConcat(likeCmd + " '" + value + "%'"));
                    break;
                case Comparison.Contains:
                    this.AddStatement(this.Context.Connection.FormatStringConcat(likeCmd + " '%" + value + "%'"));
                    break;
            }
            return this;
        }
        public QueryBuilder FilterByParam(string tableAlias, string fieldName, Comparison comparison, string paramName)
        {
            var likeCmd = "LIKE";
            if (this.Context.Connection.Type == DatabaseType.PostgreSQL)
                likeCmd = "ILIKE";
            this.StringBuilder.Append(this.FormatAndMapTable(tableAlias)).Append(".").Append(FormatAndMapField(fieldName));
            switch (comparison)
            {
                case Comparison.Equal:
                    this.AddEqualSign();
                    this.AddStatement(this.Context.Connection.FormatParameterName(paramName));
                    break;
                case Comparison.Different:
                    this.AddNotEqualSign();
                    this.AddStatement(this.Context.Connection.FormatParameterName(paramName));
                    break;
                case Comparison.Greater:
                    this.AddStatement(" > " + this.Context.Connection.FormatParameterName(paramName));
                    break;
                case Comparison.Less:
                    this.AddStatement(" < " + this.Context.Connection.FormatParameterName(paramName));
                    break;
                case Comparison.GreaterAndEqual:
                    this.AddStatement(" >= " + this.Context.Connection.FormatParameterName(paramName));
                    break;
                case Comparison.LessAndEqual:
                    this.AddStatement(" <= " + this.Context.Connection.FormatParameterName(paramName));
                    break;
                case Comparison.In:
                    throw new NotImplementedException();
                case Comparison.StartsWith:
                    this.AddStatement(this.Context.Connection.FormatStringConcat(likeCmd + " '%' + " + this.Context.Connection.FormatParameterName(paramName)));
                    break;
                case Comparison.EndsWith:
                    this.AddStatement(this.Context.Connection.FormatStringConcat(likeCmd + " " + this.Context.Connection.FormatParameterName(paramName) + " + '%'"));
                    break;
                case Comparison.Contains:
                    this.AddStatement(this.Context.Connection.FormatStringConcat(likeCmd + " '%' + " + this.Context.Connection.FormatParameterName(paramName) + " + '%'"));
                    break;
            }
            return this;
        }
        public QueryBuilder StartSubQuery()
        {
            var qb = new QueryBuilder();
            qb.Context = this.Context;
            qb.BaseQuery = this;
            return qb;
        }
        public QueryBuilder AddParamValue(object value)
        {
            this.Parameters.Add(value);
            return this;
        }
        public QueryBuilder AddParamValues(params object[] values)
        {
            if (values != null)
            {
                foreach (var item in values)
                {
                    this.Parameters.Add(item);
                }
            }
            return this;
        }
        public QueryBuilder FinishSubQuery(string alias = "")
        {
            this.BaseQuery.StringBuilder.Append("(" + this.Build() + ")");
            if (!string.IsNullOrEmpty(alias))
                this.BaseQuery.StringBuilder.Append(" AS " + this.FormatAndMapField(alias));
            return this.BaseQuery;
        }
        public QueryBuilder AddTable<T>(string alias)
        {
            this.Tables.Add(typeof(T));
            var tableName = this.Context.Connection.GetTableName(typeof(T));
            this.StringBuilder.Append(tableName).Append(" ").Append(this.Context.Connection.FormatDataElement(alias));
            return this;
        }
        public QueryBuilder AddJoin<T>(JoinType type, string alias, string joinOn, string toTableAlias, string toTableJoinOn)
        {
            this.Tables.Add(typeof(T));
            switch (type)
            {
                case JoinType.Inner:
                    this.StringBuilder.Append(" INNER JOIN ");
                    break;
                case JoinType.Left:
                    this.StringBuilder.Append(" LEFT JOIN ");
                    break;
                case JoinType.Right:
                    this.StringBuilder.Append(" RIGHT JOIN ");
                    break;
                case JoinType.LeftOuter:
                    this.StringBuilder.Append(" LEFT OUTER JOIN ");
                    break;
                case JoinType.RightOuter:
                    this.StringBuilder.Append(" RIGHT OUTER JOIN ");
                    break;
            }
            var tableName = this.Context.Connection.GetTableName(typeof(T));
            this.StringBuilder.Append(tableName).Append(" ")
                .Append(this.Context.Connection.FormatDataElement(alias))
                .Append(" ON ")
                .Append(this.Context.Connection.FormatDataElement(alias))
                .Append(".")
                .Append(this.FormatAndMapField(joinOn))
                .Append(" = ")
                .Append(this.Context.Connection.FormatDataElement(toTableAlias))
                .Append(".")
                .Append(this.FormatAndMapField(toTableJoinOn));
            return this;
        }
        public QueryBuilder AddFunction(string functionName, string tableName, string fieldName, string alias = "")
        {
            var upperFunction = functionName.ToUpper().Replace("İ", "I");
            if (this.Context.Connection.Type == DatabaseType.Oracle)
            {
                if (upperFunction == "YEAR")
                    this.AddStatement("EXTRACT(YEAR FROM TO_DATE");
                else if (upperFunction == "MONTH")
                    this.AddStatement("EXTRACT(MONTH FROM TO_DATE");
                else
                    this.AddStatement(upperFunction);
            }
            else
                this.AddStatement(upperFunction);
            this.AddLeftParanthesis();
            this.StringBuilder.Append(this.FormatAndMapTable(tableName));
            this.StringBuilder.Append(".");
            this.StringBuilder.Append(this.FormatAndMapField(fieldName));
            if (this.Context.Connection.Type == DatabaseType.Oracle)
            {
                if (upperFunction == "YEAR" || upperFunction == "MONTH")
                    this.AddStatement(", 'dd/mm/yyyy')");
            }
            this.AddRightParanthesis();
            if (!string.IsNullOrEmpty(alias))
            {
                this.StringBuilder.Append(" AS ");
                this.StringBuilder.Append(this.FormatAndMapField(alias));
            }
            return this;
        }
        public QueryBuilder AddCount(string alias = "")
        {
            this.AddStatement("COUNT(1)");
            if (!string.IsNullOrEmpty(alias))
            {
                this.StringBuilder.Append(" AS ");
                this.StringBuilder.Append(this.FormatAndMapField(alias));
            }
            return this;
        }
        public QueryBuilder SelectTableFields<T>(params string[] fieldNames)
        {
            this.LastTableName = this.Context.Connection.GetTableName(typeof(T));
            return this.SelectFields(fieldNames);
        }
        public QueryBuilder SelectTableFields(string tableName, params string[] fieldNames)
        {
            this.LastTableName = this.FormatAndMapTable(tableName);
            return this.SelectFields(fieldNames);
        }
        public QueryBuilder SelectFields(params string[] fieldNames)
        {
            if (string.IsNullOrEmpty(this.LastTableName))
                throw new Exception("Table is not added before SelectField");

            var fieldCount = 0;
            foreach (var fieldName in fieldNames)
            {
                if (fieldCount > 0)
                    this.StringBuilder.Append(", ");
                this.StringBuilder.Append(this.LastTableName);
                this.StringBuilder.Append(".");
                if (fieldName.IndexOf(" as ", StringComparison.InvariantCultureIgnoreCase) > -1)
                {
                    var splitted = fieldName.Split(new string[] { " as ", " AS ", " As ", " aS " }, StringSplitOptions.RemoveEmptyEntries);

                    this.StringBuilder.Append(this.FormatAndMapField(splitted[0]) + " AS " + this.FormatAndMapField(splitted[1]));
                }
                else
                    this.StringBuilder.Append(this.FormatAndMapField(fieldName));

                fieldCount++;
            }
            return this;
        }
        private string FormatAndMapTable(string tableName)
        {
            return this.Context.Connection.FormatDataElement(this.Context.Connection.GetMappedTableName(tableName));
        }
        private string FormatAndMapField(string fieldName)
        {
            return this.Context.Connection.FormatDataElement(this.Context.Connection.GetMappedFieldName(fieldName));
        }
        public string Build()
        {
            return this.StringBuilder.ToString();
        }
        public System.Data.DataTable GetData()
        {
            return this.GetData(1, int.MaxValue);
        }
        public System.Data.DataTable GetData(int page, int pageSize)
        {
            try
            {
                if (pageSize < int.MaxValue)
                    return this.Context.Connection.GetData(this.Build(), this.Parameters.ToArray());
                else
                    return this.Context.Connection.GetPagedData(this.Build(), page, pageSize, this.Parameters.ToArray());
            }
            catch (Exception ex)
            {
                if (!this.DesignMode)
                {
                    using (var designer = new DataDesigner())
                    {
                        this.DesignMode = true;
                        if (designer.Check(this, ex))
                        {
                            return this.GetData(page, pageSize);
                        }
                    }
                }
                this.DesignMode = false;
                throw ex;
            }
        }
        public QueryBuilder()
        {
            this.StringBuilder = new StringBuilder();
            this.Parameters = new List<object>();
            this.Tables = new List<Type>();
        }
        public void Dispose()
        {
            this.StringBuilder = null;
            this.Parameters = null;
            this.Tables = null;
            this.Context = null;
            this.LastTableName = "";
        }
        public static void Reset()
        {
            if (_Current != null)
                _Current.Dispose();
            _Current = new QueryBuilder();
        }
    }
}
