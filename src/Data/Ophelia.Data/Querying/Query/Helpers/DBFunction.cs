using System;
using System.Linq.Expressions;
using System.Runtime.Serialization;

namespace Ophelia.Data.Querying.Query.Helpers
{
    [DataContract]
    public class DBFunction : IDisposable
    {
        [DataMember]
        public string FunctionName { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public bool IsAggregiate { get; set; }
        public static DBFunction Create(string FunctionName, bool IsAggregiate, Expression expression)
        {
            return ExpressionParser.Create(expression).ToFunction(FunctionName, IsAggregiate);
        }
        public virtual void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
        
        }

        public string Build(BaseQuery query)
        {
            return this.FunctionName + "(" + query.Data.MainTable.Alias + "." + query.Context.Connection.FormatDataElement(this.Name) + ") As " + this.FunctionName + this.Name;
        }
        public DBFunction()
        {

        }
        public DBFunction Serialize()
        {
            var entity = new DBFunction();
            entity.IsAggregiate = this.IsAggregiate;
            entity.Name = this.Name;
            entity.FunctionName = this.FunctionName;
            return entity;
        }
    }
}
