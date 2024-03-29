﻿using System;
using System.Data;
using System.Linq;
using System.Linq.Expressions;

namespace Ophelia.Data.Querying.Query
{
    public class SelectQuery : BaseQuery
    {
        public SelectQuery(DataContext Context, Type EntityType) : base(Context, EntityType)
        {

        }

        public SelectQuery(DataContext Context, Model.QueryableDataSet source, Expression expression) : base(Context, source, expression)
        {

        }

        public DataTable GetData(bool retrying = false)
        {
            string query = "";
            try
            {
                if (!retrying)
                    this.VisitExpression();
                this.Data.Parameters.Clear();
                query = this.GetCommand(CommandType.None);
                var data = this.Context.Connection.GetData(query, this.Data.SkippedCount, this.Data.PageSize, this.Data.Parameters.ToArray());
                this.DesignMode = false;
                return data;
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
                            return this.GetData(true);
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

        protected override string GetCommand(CommandType cmdType)
        {
            if (cmdType == CommandType.Identity)
            {
                return "SELECT @@IDENTITY";
            }
            if (this.Data.EntityType.Name.StartsWith("IGrouping") || this.Data.EntityType.Name.StartsWith("OGrouping"))
            {
                this.Data.MainTable = new Helpers.Table(this, this.Data.EntityType.GenericTypeArguments[1]);
            }
            else
                this.Data.MainTable = new Helpers.Table(this, this.Data.EntityType);

            if (this.Data.MainTable.EntityType.IsPrimitiveType())
                throw new Exception("Primitive Types can not be queried.");

            var strWhere = this.BuildWhereString();
            var strInclude = this.BuildIncludeString();
            var strGroup = this.BuildGroupByString();
            var strOrder = this.BuildOrderByString();
            var strFunctions = this.BuildFunctionsSelectString();
            var strSelectedFields = this.BuildSelectedFieldsString();
            if (!string.IsNullOrEmpty(strInclude) && this.Data.Selectors.Any())
            {
                strSelectedFields += ("," + strInclude);
                strSelectedFields = strSelectedFields.Trim(',');
            }

            var sb = new System.Text.StringBuilder();
            sb.Append("SELECT ");
            if (cmdType == CommandType.Count)
            {
                sb.Append(this.BuildCountString() + " As " + this.Context.Connection.FormatDataElement("Count"));
            }
            else
            {
                if (this.Data.DistinctEnabled && this.Data.Selectors.Any())
                    sb.Append("DISTINCT ");

                bool hasField = false;
                if (this.Data.Grouping)
                {
                    sb.Append(this.BuildGroupBySelectString());
                    hasField = true;
                }
                else
                {
                    if (!string.IsNullOrEmpty(strSelectedFields))
                    {
                        if (hasField)
                            sb.Append(",");
                        sb.Append(strSelectedFields);
                        hasField = true;
                    }
                    else
                    {
                        if (!this.Data.Functions.Where(op => op.IsAggregiate).Any())
                        {
                            if (!string.IsNullOrEmpty(strInclude))
                            {
                                sb.Append(strInclude);
                                hasField = true;
                            }

                            if (hasField)
                                sb.Append(",");
                            sb.Append(this.Context.Connection.GetAllSelectFields(this.Data.MainTable, false));
                            hasField = true;
                        }
                    }
                }
                if (!string.IsNullOrEmpty(strFunctions))
                {
                    if (hasField)
                        sb.Append(",");
                    sb.Append(strFunctions);
                }
            }
            sb.Append(" FROM ");
            sb.Append(this.Data.MainTable.FullName);
            foreach (var join in this.Data.MainTable.Joins)
            {
                sb.Append(join.BuildJoinString());
            }
            sb.Append(strWhere);
            sb.Append(strGroup);
            if (cmdType == CommandType.None)
            {
                if (!this.Data.Functions.Where(op => op.IsAggregiate).Any())
                    sb.Append(strOrder);
            }
            if (this.Data.Grouping && string.IsNullOrEmpty(strOrder))
            {
                sb.Append($" ORDER BY {this.Context.Connection.FormatDataElement("Count")} DESC");
            }
            if (this.Data.Grouping && cmdType == CommandType.Count)
            {
                return $"SELECT Count(1) AS {this.Context.Connection.FormatDataElement("Count")} FROM ({sb.ToString()}) GroupedTable";
            }
            else
                return sb.ToString();
        }
    }
}
