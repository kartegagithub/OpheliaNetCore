using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ophelia.Service
{
    public class WebServiceCollectionRequest : WebServiceObjectRequest
    {
        //public System.Xml.Linq.XElement XExpression { get; set; }
        //public System.Xml.Linq.XElement BaseXExpression { get; set; }
        public Ophelia.Data.Querying.Query.QueryData QueryData { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public new WebServiceCollectionRequest AddParam(string key, object value)
        {
            return (WebServiceCollectionRequest)base.AddParam(key, value);
        }
        //public System.Linq.Expressions.Expression Expression
        //{
        //    get
        //    {
        //        if (this.XExpression != null)
        //        {
        //            var serializer = new Ophelia.Linq.Serialization.ExpressionSerializer();
        //            return serializer.Deserialize(this.XExpression);
        //        }
        //        return null;
        //    }
        //}
        public WebServiceCollectionRequest()
        {
            
        }
    }
}
