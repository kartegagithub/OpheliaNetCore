using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Ophelia.Web.View.Mvc.Controls.Binders.CollectionBinder
{
    public class Grouper<T> where T: class
    {
        public Grouper()
        {
            this.CanTranslateText = true;
        }
        public Type Type { get; set; }
        public string Text { get; set; }
        public bool CanTranslateText { get; set; }
        public Func<T, object> DisplayMemberExpression { get; set; }
        private string formattedName = "";
        public string FormatRequestName()
        {
            return "Grouper-" + this.FormatName();
        }
        public string FormatName()
        {
            if (!string.IsNullOrEmpty(formattedName))
                return formattedName;
            if (this.Expression != null)
            {
                var path = this.Expression.ParsePath();
                if (path.IndexOf("(") > -1)
                    path = path.Replace("(", "").Replace(")", "");
                if(this.Expression.Body.Type.IsClass && !this.Expression.Body.Type.FullName.Contains("System."))
                    path += "ID";
                if (path.IndexOf(".") > -1)
                {
                    var tmp = path.Split('.');
                    path = tmp[tmp.Length - 2] + "ID";
                }
                formattedName = path;
            }
            return formattedName;
        }
        public string FormatText(Client Client)
        {
            if (!string.IsNullOrEmpty(this.Text))
            {
                if (this.CanTranslateText)
                    return Client.TranslateText(this.Text);
                else
                    return Text;
            }
            else if (this.Expression != null)
            {
                return Client.TranslateText(this.FormatName());
            }
            return "";
        }
        public Expression<Func<T, object>> Expression { get; set; }
        public bool IsSelected { get; set; }
    }
}
