﻿using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Ophelia.Data.Querying.Query
{
    public abstract class BaseQuery : IDisposable
    {
        private DataContext _Context;
        public QueryData Data { get; set; }
        internal bool DesignMode { get; set; }
        private int TableJoinIndex = 0;
        protected Expression? Expression { get; set; }
        private QueryData dataToExtend { get; set; }
        public DataContext Context
        {
            get
            {
                return this._Context;
            }
        }

        public TResult Execute<TResult>()
        {
            return this.Execute<TResult>(CommandType.None);
        }

        public TResult Execute<TResult>(CommandType cmdType)
        {
            string query = "";
            try
            {
                this.VisitExpression();
                this.Data.Parameters.Clear();
                query = this.GetCommand(cmdType);
                TResult returnVal = default(TResult);
                if (!string.IsNullOrEmpty(query))
                {
                    if (this.GetType().IsAssignableFrom(typeof(SelectQuery)) || (this.GetType().IsAssignableFrom(typeof(InsertQuery)) && (this.Context.Connection.Type == DatabaseType.MySQL || this.Context.Connection.Type == DatabaseType.SQLServer)))
                    {
                        var tmp = this.Context.Connection.ExecuteScalar(query, this.Data.Parameters.ToArray());
                        if (tmp != DBNull.Value)
                            returnVal = (TResult)Convert.ChangeType(tmp, typeof(TResult));
                    }
                    else
                    {
                        var tmp = this.Context.Connection.ExecuteNonQuery(query, this.Data.Parameters.ToArray());
                        if (tmp != DBNull.Value)
                            returnVal = (TResult)Convert.ChangeType(tmp, typeof(TResult));
                    }
                }
                this.OnAfterExecute();
                this.DesignMode = false;
                return returnVal;
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
                            return this.Execute<TResult>(cmdType);
                        }
                    }
                }
                this.DesignMode = false;
                throw;
            }
            finally
            {
                query = "";
            }
        }
        protected virtual void OnAfterExecute()
        {

        }
        protected abstract string GetCommand(CommandType cmdType);

        public virtual void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        public string GetTableJoinIndex()
        {
            if (this.TableJoinIndex == 0)
                this.TableJoinIndex = new Random().Next(20, 50);

            this.TableJoinIndex++;
            return this.TableJoinIndex.ToString();
        }

        protected virtual void Dispose(bool disposing)
        {

        }

        public BaseQuery(DataContext Context, Type EntityType)
        {
            this._Context = Context;
            this.Data = new QueryData();
            if (EntityType.Name.StartsWith("IGrouping") || EntityType.Name.StartsWith("OGrouping"))
                this.Data.EntityType = EntityType.GenericTypeArguments[1];
            else
                this.Data.EntityType = EntityType;
        }

        public BaseQuery(DataContext Context, Model.QueryableDataSet source, Expression expression) : this(Context, source.InnerType != null ? source.InnerType : source.ElementType)
        {
            this.Expression = expression;
            this.dataToExtend = source.ExtendedData;
            this.Data.DistinctEnabled = source.DistinctEnabled;
        }
        protected void VisitExpression()
        {
            var visitor = new SQLPreparationVisitor(this.Data);
            visitor.Visit(this.Expression);
            this.Extend();
        }
        protected void Extend()
        {
            if (this.dataToExtend != null)
            {
                this.Data.GroupPageSize = this.dataToExtend.GroupPageSize;
                this.Data.GroupPagination = this.dataToExtend.GroupPagination;
                this.Data.Groupers.AddRange(this.dataToExtend.Groupers);
                this.Data.Includers.AddRange(this.dataToExtend.Includers);
                if (this.dataToExtend.Sorters.Count > 0)
                    this.Data.Sorters = this.dataToExtend.Sorters;
                this.Data.Functions.AddRange(this.dataToExtend.Functions);
                if (this.dataToExtend.Filter != null)
                {
                    if (this.Data.Filter != null)
                    {
                        this.Data.Filter = new Helpers.Filter()
                        {
                            Left = this.dataToExtend.Filter,
                            Right = this.Data.Filter,
                            Constraint = Constraint.And
                        };
                    }
                    else
                    {
                        this.Data.Filter = this.dataToExtend.Filter;
                    }
                }
                //this.dataToExtend = null;
            }
        }
        protected string BuildWhereString()
        {
            if (this.Data.Filter != null)
            {
                var sb = new StringBuilder();
                var str = sb.Append(" WHERE ").Append(this.Data.Filter.Build(this)).ToString();
                if (str.Trim() == "WHERE")
                    return "";
                return str;
            }
            return "";
        }

        protected string BuildGroupByString()
        {
            if (this.Data.Groupers.Count > 0)
            {
                var sb = new StringBuilder();
                foreach (var grouper in this.Data.Groupers)
                {
                    sb.Append(grouper.Build(this));
                    sb.Append(",");
                }
                var tmp = sb.ToString().Trim(',');
                if (!string.IsNullOrEmpty(tmp))
                    return " GROUP BY " + tmp;
            }
            return "";
        }
        protected string BuildGroupBySelectString()
        {
            if (this.Data.Groupers.Count > 0)
            {
                var sb = new StringBuilder();
                foreach (var grouper in this.Data.Groupers)
                {
                    sb.Append(grouper.Build(this, true));
                    sb.Append(",");
                }
                sb.Append("COUNT(1) As " + this.Context.Connection.FormatDataElement("Count"));
                return sb.ToString().Trim(',');
            }
            return "";
        }
        protected string BuildFunctionsSelectString()
        {
            if (this.Data.Functions.Count > 0)
            {
                var sb = new StringBuilder();
                foreach (var f in this.Data.Functions)
                {
                    sb.Append(f.Build(this));
                    sb.Append(",");
                }
                return sb.ToString().Trim(',');
            }
            return "";
        }
        protected string BuildOrderByString()
        {
            if (this.Data.Sorters.Count > 0)
            {
                var sb = new StringBuilder();
                foreach (var sorter in this.Data.Sorters)
                {
                    if (this.Data.DistinctEnabled && this.Data.Selectors.Any() && !string.IsNullOrEmpty(sorter.Name) && !this.Data.Selectors.Any(op => !string.IsNullOrEmpty(op.Name) && op.Name.Equals(sorter.Name, StringComparison.InvariantCultureIgnoreCase)))
                        continue;

                    var tmp2 = sorter.Build(this);
                    if (!string.IsNullOrEmpty(tmp2))
                    {
                        sb.Append(tmp2);
                        sb.Append(",");
                    }
                    tmp2 = "";
                }
                var tmp = sb.ToString().Trim(',');
                if (!string.IsNullOrEmpty(tmp))
                    return " ORDER BY " + tmp;
            }
            return "";
        }

        protected string BuildIncludeString()
        {
            if (this.Data.Includers.Count > 0)
            {
                var sb = new StringBuilder();
                foreach (var includer in this.Data.Includers)
                {
                    if (this.IncluderIsSelected(includer) || this.Data.Sorters.Any(op => op.Name.StartsWith(includer.Name + ".")))
                    {
                        sb.Append(includer.Build(this));
                        sb.Append(",");
                    }
                }
                return sb.ToString().Trim(',');
            }
            return "";
        }
        internal bool IncluderIsSelected(Helpers.Includer includer)
        {
            if (!this.Data.Selectors.Any())
                return true;
            else
            {
                foreach (var item in this.Data.Selectors)
                {
                    if (this.IncluderIsSelected(includer, item))
                        return true;
                }
                return false;
            }
        }
        internal bool IncluderIsSelected(Helpers.Includer includer, Helpers.Selector selector)
        {
            if (!string.IsNullOrEmpty(includer.Name) && includer.Name == selector.Name)
                return true;
            else if (includer.PropertyInfo != null && selector.PropertyInfo != null && includer.PropertyInfo == selector.PropertyInfo)
                return true;
            else if (selector.Members != null && selector.Members.Any())
            {
                foreach (PropertyInfo item in selector.Members)
                {
                    if (item == includer.PropertyInfo || item.DeclaringType == includer.PropertyInfo.PropertyType) return true;
                }
            }
            return false;
        }
        protected string BuildSelectedFieldsString()
        {
            if (this.Data.Selectors.Count > 0)
            {
                var sb = new StringBuilder();
                foreach (var selector in this.Data.Selectors)
                {
                    sb.Append(selector.Build(this));
                    sb.Append(",");
                }
                return sb.ToString().Trim(',');
            }
            return "";
        }

        protected string BuildCountString()
        {
            if (this.Data.Selectors.Count > 0)
            {
                var sb = new StringBuilder();
                sb.Append("Count(DISTINCT ");
                foreach (var selector in this.Data.Selectors)
                {
                    sb.Append(selector.Build(this));
                    sb.Append(",");
                }
                return sb.ToString().Trim(',') + ")";
            }
            return "Count(1)";
        }

        protected string BuildPKWhere(object entity)
        {
            var pks = Extensions.GetPrimaryKeyProperties(this.Data.EntityType);
            var sb = new StringBuilder();
            foreach (var pkPRoperty in pks)
            {
                sb.Append(this.Context.Connection.FormatDataElement(this.Context.Connection.GetMappedFieldName(Extensions.GetColumnName(pkPRoperty))));
                sb.Append(" = " + this.Context.Connection.FormatParameterName("p") + this.Data.Parameters.Count);
                this.Data.Parameters.Add(pkPRoperty.GetValue(entity));
                if (pkPRoperty != pks.LastOrDefault())
                    sb.Append(" AND ");
            }
            return sb.ToString();
        }
    }
}
