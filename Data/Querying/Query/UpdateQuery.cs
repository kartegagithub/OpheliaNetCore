using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;

namespace Ophelia.Data.Querying.Query
{
    public class UpdateQuery : BaseQuery
    {
        private Model.DataEntity Entity;
        private Expressions.UpdateExpression[] Updaters;
        public UpdateQuery(DataContext Context, Model.DataEntity Entity) : base(Context, Entity.GetType())
        {
            this.Entity = Entity;
        }

        public UpdateQuery(DataContext Context, Model.QueryableDataSet source, MethodCallExpression expression) : base(Context, source, expression)
        {

        }

        public UpdateQuery(DataContext Context, Model.QueryableDataSet source, MethodCallExpression expression, Expressions.UpdateExpression updater) : base(Context, source, expression)
        {
            this.Updaters = new Expressions.UpdateExpression[] { updater };
        }
        public UpdateQuery(DataContext Context, Model.QueryableDataSet source, MethodCallExpression expression, Expressions.UpdateExpression[] updaters) : base(Context, source, expression)
        {
            this.Updaters = updaters;
        }
        protected override string GetCommand(CommandType cmdType)
        {
            this.Data.MainTable = new Helpers.Table(this, this.Data.EntityType);
            var relationClassProperty = this.Data.EntityType.GetCustomAttributes(typeof(Attributes.RelationClass)).FirstOrDefault() as Attributes.RelationClass;

            var sb = new StringBuilder();

            sb.Append("UPDATE ");
            sb.Append(this.Context.Connection.GetTableName(this.Data.EntityType));
            sb.Append(" SET ");

            if (this.Entity != null)
            {
                var changedProperties = this.Entity.Tracker.GetChanges();
                if (changedProperties != null && changedProperties.Count > 0)
                {
                    int i = 0;
                    foreach (var _prop in changedProperties)
                    {
                        if (relationClassProperty == null || this.Data.EntityType.IsAssignableFrom(_prop.PropertyInfo.DeclaringType))
                        {
                            if (!_prop.PropertyInfo.PropertyType.IsDataEntity() && !_prop.PropertyInfo.PropertyType.IsQueryableDataSet() && !_prop.PropertyInfo.PropertyType.IsQueryable())
                            {
                                if (i != 0)
                                    sb.Append(", ");
                                sb.Append(this.Context.Connection.FormatDataElement(this.Context.Connection.GetMappedFieldName(_prop.PropertyInfo.Name)));
                                sb.Append(" = ");
                                sb.Append(this.Context.Connection.FormatParameterName("p") + this.Data.Parameters.Count());
                                if (_prop.Value == null)
                                {
                                    this.Data.Parameters.Add(DBNull.Value);
                                }
                                else
                                    this.Data.Parameters.Add(this.Context.Connection.FormatParameterValue(_prop.Value));
                                i++;
                            }
                        }
                    }
                }
            }
            else if (this.Updaters != null && this.Updaters.Length > 0)
            {
                var counter = 0;
                foreach (var Updater in this.Updaters)
                {
                    if (counter > 0)
                        sb.Append(",");
                    sb.Append(this.Context.Connection.FormatDataElement(this.Context.Connection.GetMappedFieldName(Updater.Expression.ParsePath())));
                    sb.Append(" = ");
                    sb.Append(this.Context.Connection.FormatParameterName("p") + this.Data.Parameters.Count());
                    this.Data.Parameters.Add(Updater.Value);
                    counter++;
                }
            }
            if (this.Entity != null)
            {
                sb.Append(" WHERE ");
                sb.Append(this.Context.Connection.GetPrimaryKeyName(this.Data.EntityType));
                sb.Append(" = " + this.Context.Connection.FormatParameterName("p") + this.Data.Parameters.Count);
                this.Data.Parameters.Add(this.Entity.ID);
            }
            else
            {
                var strWhere = this.BuildWhereString();
                if (this.Data.MainTable.Joins.Count > 0)
                {
                    sb.Append(" FROM ");
                    sb.Append(this.Data.MainTable.FullName);
                    foreach (var join in this.Data.MainTable.Joins)
                    {
                        sb.Append(join.BuildJoinString());
                    }
                    sb.Append(" ");
                }
                if (!string.IsNullOrEmpty(strWhere))
                {
                    if (this.Data.MainTable.Joins.Count > 0)
                    {
                        sb.Append(strWhere);
                        sb.Append(" AND ");
                        sb.Append(this.Data.MainTable.Name + "." + this.Context.Connection.GetPrimaryKeyName(this.Data.EntityType));
                        sb.Append(" = ");
                        sb.Append(this.Data.MainTable.Alias + "." + this.Context.Connection.GetPrimaryKeyName(this.Data.EntityType));
                    }
                    else
                    {
                        sb.Append(strWhere.Replace(this.Data.MainTable.Alias + ".", ""));
                    }
                }
            }
            return sb.ToString();
        }
    }
}
