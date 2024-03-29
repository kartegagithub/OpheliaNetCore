﻿using System.Linq.Expressions;
using System.Text;

namespace Ophelia.Data.Querying.Query
{
    public class DeleteQuery : BaseQuery
    {
        private object Entity;

        public DeleteQuery(DataContext Context, object Entity) : base(Context, Entity.GetType())
        {
            this.Entity = Entity;
        }

        public DeleteQuery(DataContext Context, Model.QueryableDataSet source, Expression expression) : base(Context, source, expression)
        {

        }

        protected override string GetCommand(CommandType cmdType)
        {
            this.Data.MainTable = new Helpers.Table(this, this.Data.EntityType);

            var sb = new StringBuilder();

            sb.Append("DELETE FROM ");
            sb.Append(this.Context.Connection.GetTableName(this.Data.EntityType));
            if (this.Entity != null)
            {
                sb.Append(" WHERE ");
                sb.Append(this.BuildPKWhere(this.Entity));
            }
            else
            {
                var strWhere = this.BuildWhereString();
                if (this.Data.MainTable.Joins.Count > 0)
                {
                    if (this.Context.Connection.Type == DatabaseType.PostgreSQL)
                    {
                        sb.Append(" USING ");
                        sb.Append(this.Data.MainTable.Name);
                        sb.Append(" AS ");
                        sb.Append(this.Data.MainTable.Alias);
                    }
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
