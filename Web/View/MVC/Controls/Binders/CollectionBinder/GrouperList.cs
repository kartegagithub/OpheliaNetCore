using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Ophelia.Web.View.Mvc.Controls.Binders.CollectionBinder
{
    public class GrouperList<T> : List<Grouper<T>>
        where T : class
    {
        public Grouper<T> Add(Expression<Func<T, object>> exp, string Text, Type type)
        {
            var item = new Grouper<T>() { Expression = exp };
            item.Text = Text;
            item.Type = type;
            this.Add(item);
            return item;
        }
        public Grouper<T> Add(Expression<Func<T, object>> exp, string Text, Func<T, object> displayMemberExpression)
        {
            var item = new Grouper<T>() { Expression = exp };
            item.Text = Text;
            item.DisplayMemberExpression = displayMemberExpression;
            this.Add(item);
            return item;
        }
        public Grouper<T> Add(Expression<Func<T, object>> exp, string Text)
        {
            var item = new Grouper<T>() { Expression = exp };
            item.Text = Text;
            this.Add(item);
            return item;
        }
        public Grouper<T> Add(Expression<Func<T, object>> exp)
        {
            return this.Add(exp, "");
        }
    }
}
