using Ophelia.Data;
using Ophelia.Data.Model;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Ophelia.Data.Querying.Query
{
    public class InsertQuery : BaseQuery
    {
        private object Entity;
        private Dictionary<string, object> SequenceValue = new Dictionary<string, object>();
        protected override void OnAfterExecute()
        {
            base.OnAfterExecute();
            //TODO: MySQL Sequences are created but not set.
            if (this.SequenceValue.Any())
            {
                var pks = Extensions.GetPrimaryKeyProperties(this.Data.EntityType);
                foreach (var pk in pks)
                {
                    if (this.SequenceValue.ContainsKey(pk.Name))
                        pk.SetValue(this.Entity, this.SequenceValue[pk.Name]);
                }
            }
        }
        public InsertQuery(DataContext Context, object Entity) : base(Context, Entity.GetType())
        {
            this.Entity = Entity;
        }

        public InsertQuery(DataContext Context, Model.QueryableDataSet source, Expression expression) : base(Context, source, expression)
        {

        }

        protected override string GetCommand(CommandType cmdType)
        {
            var relationClassProperty = this.Data.EntityType.GetCustomAttributes(typeof(Attributes.RelationClass)).FirstOrDefault() as Attributes.RelationClass;

            var changedProperties = (this.Entity.GetPropertyValue("Tracker") as PocoEntityTracker)?.GetChanges();
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

                                sbFields.Append(this.Context.Connection.FormatDataElement(this.Context.Connection.GetMappedFieldName(Extensions.GetColumnName(_prop.PropertyInfo))));
                                sbValues.Append(this.Context.Connection.FormatParameterName("p") + i);
                                this.Data.Parameters.Add(this.Context.Connection.FormatParameterValue(_prop.Value));
                                i++;
                            }
                        }
                    }
                }
                var pks = Extensions.GetPrimaryKeyProperties(this.Data.EntityType);
                foreach (var pk in pks)
                {
                    if (!Extensions.IsComputedProperty(pk))
                    {
                        //ID AutoIncrement: IsIdentityProperty = true, DBIncrementedIdentityColumn = true -> Veri tabanı incremente edecek
                        //ID AutoIncrement: IsIdentityProperty = true, DBIncrementedIdentityColumn = false -> Ophelia incremente edecek

                        if (!Extensions.IsIdentityProperty(pk) || !this.Context.Configuration.DBIncrementedIdentityColumn)
                        {
                            sbFields.Append(",");
                            sbFields.Append(this.Context.Connection.FormatDataElement(this.Context.Connection.GetMappedFieldName(Extensions.GetColumnName(pk))));
                            sbValues.Append(",");
                            sbValues.Append(this.Context.Connection.FormatParameterName("p") + this.Data.Parameters.Count);
                            if (Extensions.IsIdentityProperty(pk) && !this.Context.Configuration.DBIncrementedIdentityColumn)
                            {
                                this.SequenceValue[pk.Name] = this.Context.Connection.GetSequenceNextVal(this.Entity.GetType(), pk, pks.Count == 1);
                                this.Data.Parameters.Add(this.SequenceValue[pk.Name]);
                            }
                            else
                            {
                                this.Data.Parameters.Add(pk.GetValue(this.Entity));
                            }
                        }
                    }
                }

                sb.Append("INSERT INTO ");
                sb.Append(this.Context.Connection.GetTableName(this.Data.EntityType));
                sb.Append("(");
                sb.Append(sbFields);
                sb.Append(")");
                sb.Append(" VALUES(");
                sb.Append(sbValues);
                sb.Append(")");
                if (this.Context.Connection.Type == DatabaseType.SQLServer)
                {
                    sb.Append("; SELECT @@IDENTITY;");
                }
                else if (this.Context.Connection.Type == DatabaseType.MySQL)
                {
                    sb.Append("; SELECT LAST_INSERT_ID();");
                }
                if (this.Context.Connection.Type == DatabaseType.PostgreSQL && this.Context.Configuration.DBIncrementedIdentityColumn)
                {
                    var idProp = Ophelia.Data.Extensions.GetPrimaryKeyProperty(this.Data.EntityType);
                    sb.Append($" RETURNING \"{idProp.Name}\";");
                }
                sbValues = null;
                sbFields = null;
                return sb.ToString();
            }
            return "";
        }
    }
}
