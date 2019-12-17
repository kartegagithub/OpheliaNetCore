using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ophelia.Web.View.Mvc.Controls.Binders.Fields;
using Ophelia.Web.View.Mvc.Models;

namespace Ophelia.Web.View.Mvc.Controls.Binders.CollectionBinder
{
    public class FilterPanel<Model, T> : FieldContainer<Model> 
        where T : class 
        where Model: ListModel<T>
    {
        private CollectionBinder<Model, T> Binder { get; set; }
        public bool ShowUserCreatedIDFilter { get; set; }

        public override Model Entity
        {
            get
            {
                return this.Binder.DataSource;
            }
        }

        public override Client Client
        {
            get
            {
                return this.Binder.Client;
            }
        }

        public override string[] DefaultEntityProperties
        {
            get
            {
                return this.Binder.DefaultEntityProperties;
            }
        }

        public override int CurrentLanguageID
        {
            get
            {
                return this.Binder.Client.CurrentLanguageID;
            }
        }

        public FilterPanel(CollectionBinder<Model, T> binder)
        {
            this.Binder = binder;
            this.ShowUserCreatedIDFilter = true;
        }

        public override BaseField<Model> AddField(BaseField<Model> field)
        {
            field.LabelControl.CssClass = "filter-label collapsed control-label col-lg-12";
            field.DataControlParent.CssClass = "filter-control col-lg-12";
            this.Controls.Add(field);
            return field;
        }

        public override bool CanDrawField(BaseField<Model> field)
        {
            return true;
        }
    }
}
