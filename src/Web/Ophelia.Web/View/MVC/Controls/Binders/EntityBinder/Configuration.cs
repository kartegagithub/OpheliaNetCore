﻿using System;

namespace Ophelia.Web.View.Mvc.Controls.Binders.EntityBinder
{
    public class Configuration : IDisposable
    {
        public bool ReadOnly { get; set; }
        public bool AllowNew { get; set; }
        public bool AllowSave { get; set; }
        public bool AllowDelete { get; set; }
        public bool AllowEdit { get; set; }
        public bool AllowSettings { get; set; }
        public bool AllowRollback { get; set; }
        public bool AllowHistory { get; set; }
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

        public string EditButtonText { get; set; }
        public string EditButtonIcon { get; set; }
        public string EditButtonClass { get; set; }

        public string BackButtonText { get; set; }

        public string SaveButtonText { get; set; }
        public string SaveButtonIcon { get; set; }
        public string SaveButtonClass { get; set; }

        public string DeleteButtonText { get; set; }
        public string DeleteButtonIcon { get; set; }
        public string DeleteButtonClass { get; set; }

        public string NewButtonText { get; set; }
        public string NewButtonIcon { get; set; }
        public string NewButtonClass { get; set; }

        public string SettingsButtonText { get; set; }
        public string SettingsButtonIcon { get; set; }

        public string RollbackButtonText { get; set; }
        public string RollbackButtonIcon { get; set; }
        public string RollbackButtonClass { get; set; }

        public string HistoryButtonText { get; set; }
        public string HistoryButtonIcon { get; set; }

        public virtual void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            this.Help.Dispose();
            this.Help = null;
        }
        public Configuration()
        {
            this.EditButtonText = "Edit";
            this.SaveButtonText = "Save";
            this.DeleteButtonText = "Delete";
            this.NewButtonText = "AddNew";
            this.BackButtonText = "Back";
            this.SettingsButtonText = "Settings";
            this.RollbackButtonText = "Rollback";
            this.HistoryButtonText = "History";
            this.HistoryButtonIcon = "icon-history";
            this.SaveButtonIcon = "icon-floppy-disk";
            this.SaveButtonClass = "save-button";
            this.DeleteButtonIcon = "icon-trash";
            this.DeleteButtonClass = "delete-button";
            this.SettingsButtonIcon = "icon-gear";
            this.RollbackButtonClass = "roll-back-button";
            this.RollbackButtonIcon = "icon-undo";
            this.NewButtonIcon = "icon-plus2";
            this.NewButtonClass = "new-button";
            this.EditButtonIcon = "icon-pencil5";
            this.EditButtonClass = "edit-button";

            this.Help = new HelpConfiguration();
            this.AllowDelete = true;
            this.AllowNew = true;
            this.AllowSave = true;
            this.AllowSettings = true;
            this.AllowRollback = false;
            this.HideTabHeader = false;
            this.AllowEdit = false;
            this.ShowDefaultFields = true;
            this.AllowNextItemNavigation = true;
            this.AllowPrevItemNavigation = true;
            this.AllowHistory = true;
            this.TabPaneCssClass = "tab-pane fade content-group col-sm-12 no-padding";
        }
    }
}
