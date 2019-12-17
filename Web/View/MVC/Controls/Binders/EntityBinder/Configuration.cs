using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ophelia.Web.View.Mvc.Controls.Binders.EntityBinder
{
    public class Configuration: IDisposable
    {
        public bool ReadOnly { get; set; }
        public bool AllowNew { get; set; }
        public bool AllowSave { get; set; }
        public bool AllowDelete { get; set; }
        public bool AllowEdit { get; set; }
        public bool AllowSettings { get; set; }
        public bool AllowNextItemNavigation { get; set; }
        public bool AllowPrevItemNavigation { get; set; }
        public bool ShowDefaultFields { get; set; }
        public bool HideTabHeader { get; set; }
        public string EditURL { get; set; }
        public string NewURL { get; set; }
        public string ViewURL { get; set; }
        public string InitialMode { get; set; }
        public bool AllowModeChange { get; set; }
        public HelpConfiguration Help { get; set; }
        public string DataControlParentCssClass { get; set; }
        public string LabelCssClass { get; set; }
        public string TabPaneCssClass { get; set; }
        public void AddModeField(string Mode)
        {

        }

        public void Dispose()
        {
            this.Help.Dispose();
            this.Help = null;
        }

        public Configuration()
        {
            this.Help = new HelpConfiguration();
            this.AllowDelete = true;
            this.AllowNew = true;
            this.AllowSave = true;
            this.AllowSettings = true;
            this.HideTabHeader = false;
            this.AllowEdit = false;
            this.ShowDefaultFields = true;
            this.AllowNextItemNavigation = true;
            this.AllowPrevItemNavigation = true;
            this.TabPaneCssClass = "tab-pane fade content-group col-sm-12 no-padding";
        }
    }
}
