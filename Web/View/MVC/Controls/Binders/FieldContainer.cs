using Ophelia.Web.UI.Controls;

namespace Ophelia.Web.View.Mvc.Controls.Binders
{
    public abstract class FieldContainer<T> : Panel where T : class
    {
        public abstract T Entity { get; }
        public abstract Client Client { get; }
        public abstract Fields.BaseField<T> AddField(Fields.BaseField<T> field);
        public abstract bool CanDrawField(Fields.BaseField<T> field);
        public abstract string[] DefaultEntityProperties { get; }
        public abstract int CurrentLanguageID { get; }
        public string DataControlParentCssClass { get; set; }
        public string LabelCssClass { get; set; }
        public string FieldParentCssClass { get; set; }
        public string DecimalFormat { get; set; }
        public string IntFormat { get; set; }
        public Fields.BaseField<T> GetField(string Name)
        {
            foreach (var item in this.Controls)
            {
                if (((Fields.BaseField<T>)item).Text == Name)
                    return (Fields.BaseField<T>)item;
            }
            return null;
        }

        public void MoveFieldToLast(string Name)
        {
            var field = this.GetField(Name);
            if (field != null)
            {
                this.Controls.Remove(field);
                this.Controls.Add(field);
            }
        }
        public virtual void ValidateSelectedValue(Fields.BaseField<T> Field)
        {

        }
        public virtual void SetFieldProperties(Fields.BaseField<T> Field)
        {

        }
    }
}
