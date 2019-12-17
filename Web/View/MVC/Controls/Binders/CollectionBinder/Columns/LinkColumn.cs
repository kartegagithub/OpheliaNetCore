using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ophelia.Web.UI.Controls;

using System.Linq.Expressions;
using Ophelia.Web.View.Mvc.Models;

namespace Ophelia.Web.View.Mvc.Controls.Binders.CollectionBinder.Columns
{
    public class LinkColumn<TModel, T> : BaseColumn<TModel, T> where T:class where TModel : ListModel<T>
    {
        public LinkColumn(CollectionBinder<TModel, T> binder, string Name) : base(binder, Name)
        {
            
        }
    }
}
