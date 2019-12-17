using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;
using Ophelia.Data;

namespace Ophelia.Data.Querying.Query
{
    public class InsertQuery : BaseQuery
    {
        private Model.DataEntity Entity;
        private long SequenceValue = 0;
        protected override void OnAfterExecute()
        {
            base.OnAfterExecute();
            if (this.SequenceValue > 0)
                this.Entity.ID = this.SequenceValue;
        }
        public InsertQuery(DataContext Context, Model.DataEntity Entity) : base(Context, Entity.GetType())
        {
            this.Entity = Entity;
        }

        public InsertQuery(DataContext Context, Model.QueryableDataSet source, MethodCallExpression expression) : base(Context, source, expression)
        {

        }

        protected override string GetCommand(CommandType cmdType)
        {
            var relationClassProperty = this.Data.EntityType.GetCustomAttributes(typeof(Attributes.RelationClass)).FirstOrDefault() as Attributes.RelationClass;

            var changedProperties = this.Entity.Tracker.GetChanges();
            if (changedProperties != null && changedProperties.Count > 0)
            {
                var sb = new StringBuilder();
                var sbFields = new StringBuilder();
                var sbValues = new StringBuilder();

                int i = 0;
                foreach (var _prop in changedProperties)
                {
                    if (relationClassProperty == null || this.Data.EntityType.IsAssignableFrom(_prop.PropertyInfo.DeclaringType))
                    {
                        if (!_prop.PropertyInfo.PropertyType.IsDataEntity() && !_prop.PropertyInfo.PropertyType.IsPOCOEntity() && !_prop.PropertyInfo.PropertyType.IsQueryableDataSet() && !_prop.PropertyInfo.PropertyType.IsQueryable())
                        {
                            if (_prop.Value != null)
                            {
                                if (i != 0)
                                {
                                    sbFields.Append(", ");
                                    sbValues.Append(", ");
                                }

                                sbFields.Append(this.Context.Connection.FormatDataElement(this.Context.Connection.GetMappedFieldName(_prop.PropertyInfo.Name)));
                                sbValues.Append(this.Context.Connection.FormatParameterName("p") + i);
                                this.Data.Parameters.Add(this.Context.Connection.FormatParameterValue(_prop.Value));
                                i++;
                            }
                        }
                    }
                }
                if (this.Context.Connection.Type != DatabaseType.SQLServer && this.Context.Connection.Type != DatabaseType.MySQL)
                {
                    sbFields.Append(",");
                    sbFields.Append(this.Context.Connection.GetPrimaryKeyName(this.Entity.GetType()));
                    sbValues.Append(",");
                    sbValues.Append(this.Context.Connection.FormatParameterName("p") + this.Data.Parameters.Count);
                    this.SequenceValue = this.Context.Connection.GetSequenceNextVal(this.Entity.GetType());
                    this.Data.Parameters.Add(this.SequenceValue);
                }

                sb.Append("INSERT INTO ");
                sb.Append(this.Context.Connection.GetTableName(this.Data.EntityType));
                sb.Append("(");
                sb.Append(sbFields.ToString());
                sb.Append(")");
                sb.Append(" VALUES(");
                sb.Append(sbValues.ToString());
                sb.Append(")");
                if (this.Context.Connection.Type == DatabaseType.SQLServer)
                {
                    sb.Append("; SELECT @@IDENTITY;");
                }
                else if (this.Context.Connection.Type == DatabaseType.MySQL)
                {
                    sb.Append("; SELECT LAST_INSERT_ID();");
                }
                sbValues = null;
                sbFields = null;
                return sb.ToString();
            }
            return "";
        }
    }
}
