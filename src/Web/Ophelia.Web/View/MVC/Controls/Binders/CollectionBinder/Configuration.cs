﻿namespace Ophelia.Web.View.Mvc.Controls.Binders.CollectionBinder
{
    public class Configuration
    {
        public bool AllowSave { get; set; }
        public bool AllowSettings { get; set; }
        public bool AllowSearch { get; set; }
        public bool AllowExporting { get; set; }
        public bool AllowGrouping { get; set; }
        public bool AllowServerSideOrdering { get; set; }
        public bool AllowColumnSummarize { get; set; }
        public bool RefreshDataBeforeGroups { get; set; }
        public bool ColumnSortingByLink { get; set; }
        public bool ColumnSortingByClick { get; set; }
        public bool SaveChangesOnUIInteraction { get; set; }
        public bool EnableGroupingByDragDrop { get; set; }
        public bool AllowNew { get; set; }
        public bool AllowNewRow { get; set; }
        public bool OpenExportOptions { get; set; }
        public int MaxExportItemCount { get; set; }
        public bool DefaultFilters { get; set; }
        public string EditURL { get; set; }
        public string ViewURL { get; set; }
        public string NewURL { get; set; }
        public string ContentTableCSSClass { get; set; }
        public bool DrawViewLinkInsteadOfEdit { get; set; }
        public bool EnableColumnFiltering { get; set; }
        public bool ColumnFiltersInHead { get; set; }
        public bool Checkboxes { get; set; }
        public bool KeepSelectedCheckboxes { get; set; }
        public bool ShowCheckAll { get; set; }
        public bool ShowDefaultFields { get; set; }
        public bool RedirectOnSingleResult { get; set; }
        public string CheckboxProperty { get; set; }
        public bool AppendListOfIDOnItemLink { get; set; }
        public string CheckboxIdentifierProperty { get; set; }
        public ColumnFilteringType ColumnFilteringType { get; set; }
        public bool AddBlankColumnToStart { get; set; }
        public RowUpdateType RowUpdateType { get; set; }
        public FiltersToEntityScope FiltersToEntityScope { get; set; }
        public string SaveButtonText { get; set; }
        public string SaveButtonIcon { get; set; }
        public string SaveButtonClass { get; set; }
        public string DecimalFormat { get; set; }
        public string IntFormat { get; set; }
        public Configuration()
        {
            this.AppendListOfIDOnItemLink = true;
            this.AllowNew = true;
            this.AllowSearch = true;
            this.AllowSettings = true;
            this.AllowSave = false;
            this.AddBlankColumnToStart = true;
            this.AllowGrouping = true;
            this.AllowServerSideOrdering = false;
            this.ColumnFilteringType = ColumnFilteringType.ClientSide;
            this.RowUpdateType = RowUpdateType.None;
            this.DefaultFilters = true;
            this.ColumnFiltersInHead = true;
            this.CheckboxProperty = "IsSelected";
            this.CheckboxIdentifierProperty = "ID";
            this.ShowCheckAll = true;
            this.ShowDefaultFields = true;
            this.SaveButtonIcon = "icon-floppy-disk";
            this.SaveButtonClass = "save-button";
            this.SaveButtonText = "Save";
            this.ColumnSortingByLink = false;
            this.KeepSelectedCheckboxes = true;
            this.DecimalFormat = "N2";
            this.FiltersToEntityScope = FiltersToEntityScope.NotMappedAndManuelFiltering;
        }
    }
    public enum ColumnFilteringType
    {
        Database = 1,
        ClientSide = 2
    }
    public enum RowUpdateType
    {
        None = 0,
        CellValueChange = 1
    }

    public enum FiltersToEntityScope
    {
        All = 0,
        NotMappedAndManuelFiltering = 1,
        None = 2
    }
}
