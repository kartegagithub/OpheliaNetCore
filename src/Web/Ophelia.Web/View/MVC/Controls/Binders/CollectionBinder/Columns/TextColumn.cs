using Ophelia.Web.View.Mvc.Models;

namespace Ophelia.Web.View.Mvc.Controls.Binders.CollectionBinder.Columns
{
    public class TextColumn<TModel, T> : BaseColumn<TModel, T> where T : class where TModel : ListModel<T>
    {
        public TextColumn(CollectionBinder<TModel, T> binder, string Name) : base(binder, Name)
        {

        }
    }
}
