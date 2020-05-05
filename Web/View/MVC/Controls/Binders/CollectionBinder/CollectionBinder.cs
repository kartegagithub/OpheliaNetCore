using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Ophelia.Data;
using Ophelia.Data.Querying.Query;
using Ophelia.Reflection;
using Ophelia.Service;
using Ophelia.Web.UI.Controls;
using Ophelia.Web.View.Mvc.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Ophelia.Web.View.Mvc.Controls.Binders.CollectionBinder
{
    public class CollectionBinder<TModel, T> : Panel, IDisposable where T : class where TModel : ListModel<T>
    {
        public IUrlHelper Url
        {
            get
            {
                return this.Client.Controller.Url;
            }
        }
        public bool IsAjaxRequest
        {
            get
            {
                return this.Request.GetValue("IsAjaxRequest") == "1" || this.Request.GetValue("ajaxentitybinder") == "1";
            }
        }
        public ControllerContext ControllerContext { get; private set; }
        public HttpRequest Request { get { return this.Client.Request; } }
        public HttpResponse Response { get { return this.Client.Response; } }
        public TModel DataSource { get; private set; }
        public Client Client { get { return Client.Current; } }
        public string Title { get; set; }
        private readonly ViewContext viewContext;
        protected IQueryable<IGrouping<object, T>> GroupedData { get; set; }
        public Configuration Configuration { get; private set; }
        public FilterPanel<TModel, T> FilterPanel { get; private set; }
        public GrouperList<T> Groupers { get; private set; }
        public Panel PageTitles { get; private set; }
        public Panel HeadingElements { get; private set; }
        public List Breadcrumb { get; private set; }
        public List ActionButtons { get; private set; }
        public List<Columns.BaseColumn<TModel, T>> Columns { get; private set; }
        public bool ParentDrawsLayout { get; set; }
        public List<ServiceResultMessage> Messages { get; set; }
        protected ContentRenderMode ContentRenderMode { get; set; }
        protected virtual bool CanExport
        {
            get
            {
                return false;
            }
        }
        protected virtual string ExportOutputFileFormat
        {
            get
            {
                return "xls";
            }
        }
        protected virtual string ExportDocumentName
        {
            get
            {
                return "";
            }
        }
        public virtual string[] DefaultEntityProperties
        {
            get
            {
                return null;
            }
        }
        public CollectionBinder(ViewContext viewContext, TModel dataSource, string title) : this(dataSource, title)
        {
            if (viewContext == null)
                throw new ArgumentNullException("viewContext");

            this.viewContext = viewContext;
            this.Output = this.viewContext.Writer;
            if (this.IsAjaxRequest && this.Response != null && !this.CanExport)
            {
                this.Response.Clear();
                this.Response.ClearContent();
                this.Response.ClearHeaders();
            }
            this.SetPageSize();
            this.onViewContextSet();
            if (viewContext.ViewBag.Messages != null)
            {
                this.Messages = viewContext.ViewBag.Messages;
            }
            if (viewContext.ViewData.Model != this.DataSource)
                this.ParentDrawsLayout = true;
            this.Configure();
            this.ProcessQuery();
            this.CheckAjaxFunctions();
            this.Export();
        }
        public CollectionBinder(TModel dataSource, string title)
        {
            this.ContentRenderMode = ContentRenderMode.Normal;
            this.Configuration = new Configuration();
            this.DataSource = dataSource;
            this.Columns = new List<Binders.CollectionBinder.Columns.BaseColumn<TModel, T>>();
            this.Breadcrumb = new List();
            this.Breadcrumb.CssClass = "breadcrumb";
            this.ActionButtons = new List();
            this.ActionButtons.CssClass = "breadcrumb-elements";
            this.Groupers = new GrouperList<T>();

            this.PageTitles = new Panel();
            this.HeadingElements = new Panel();

            this.ID = title;
            this.Title = this.Client.TranslateText(title);
            this.FilterPanel = new FilterPanel<TModel, T>(this);
        }
        protected virtual void SetPageSize()
        {
            if (this.DataSource != null && this.DataSource.Pagination != null)
            {
                if (!string.IsNullOrEmpty(this.Request.GetValue(this.DataSource.Pagination.PageKey)))
                    this.DataSource.Pagination.PageNumber = this.Request.GetValue(this.DataSource.Pagination.PageKey).ToInt32();
            }
        }
        protected virtual void CheckAjaxFunctions()
        {
            if (this.IsAjaxRequest && !string.IsNullOrEmpty(this.Request.GetValue("CollectionBinderTriggerFunction")))
            {
                this.Response.Clear();
                this.Response.ClearContent();
                this.Response.ClearHeaders();
                this.Response.ContentType = "application/json";
                object result = new { success = 0, message = "" };
                switch (this.Request.GetValue("CollectionBinderTriggerFunction"))
                {
                    case "OnColumnOrderChange":
                        if (!string.IsNullOrEmpty(this.Request.GetValue("Columns")))
                        {
                            result = this.OnColumnOrderChanged(this.Request.GetValue("Columns").Split(',').ToList());
                        }
                        break;
                    case "OnCellValueChange":
                        if (this.Configuration.RowUpdateType == RowUpdateType.CellValueChange && this.DataSource.Items != null && this.DataSource.Items.Count > 0)
                        {
                            if (!string.IsNullOrEmpty(this.Request.GetValue("Column")) && this.Request.GetValue("ClassName") == GetClassName() && !string.IsNullOrEmpty(this.Request.GetValue("Identifier")))
                            {
                                var column = this.Columns.Where(op => op.FormatName() == this.Request.GetValue("Column")).FirstOrDefault();
                                if (column != null && column.IdentifierExpression != null)
                                {
                                    T item = null;
                                    item = this.DataSource.Items.Where(op => Convert.ToString(column.IdentifierExpression.GetValue(op)) == this.Request.GetValue("Identifier")).FirstOrDefault();
                                    if (item != null)
                                    {
                                        var oldValue = column.GetValue(item);
                                        if (Convert.ToString(oldValue) != this.Request.GetValue("Value"))
                                        {
                                            result = this.OnCellValueChanged(item, column, this.Request.GetValue("Value"));
                                        }
                                        else
                                            result = this.OnCellValueNotChanged(item, column, this.Request.GetValue("Value"));
                                    }
                                    else
                                        result = new { success = 0, message = this.Client.TranslateText("InvalidIdentifier") };
                                }
                                else
                                    result = new { success = 0, message = this.Client.TranslateText("InvalidColumnOrConfiguration") };
                            }
                            else
                                result = new { success = 0, message = this.Client.TranslateText("InvalidParameters") };
                        }
                        break;
                }
                this.Response.Write(result.ToJson());
                this.Response.End();
            }
        }
        public WebControl AddHeadingElementButton(string url, string text, bool openInNewWindow, string cssClass = "btn btn-link btn-float has-text")
        {
            var control = new Link() { URL = url, Text = text, NewWindow = openInNewWindow, CssClass = CssClass };
            this.HeadingElements.Controls.Add(control);
            return control;
        }
        public WebControl AddHeadingElementHtml(string html)
        {
            var control = new Literal() { Text = html };
            this.HeadingElements.Controls.Add(control);
            return control;
        }
        public WebControl AddPageTitleLink(string url, string text, bool openInNewWindow, string cssClass = "")
        {
            var control = new Link() { URL = url, Text = text, NewWindow = openInNewWindow, CssClass = CssClass };
            this.PageTitles.Controls.Add(control);
            return control;
        }
        public WebControl AddPageTitleH4(string text)
        {
            var control = new Literal() { Text = "<h4>" + text + "</h4>" };
            this.PageTitles.Controls.Add(control);
            return control;
        }
        public WebControl AddPageTitleHtml(string html)
        {
            var control = new Literal() { Text = html };
            this.PageTitles.Controls.Add(control);
            return control;
        }
        protected virtual object OnColumnOrderChanged(List<string> Columns)
        {
            return new { success = 0, message = "" };
        }

        protected virtual object OnCellValueNotChanged(T item, Columns.BaseColumn<TModel, T> Column, string Value)
        {
            return new { success = 0, message = this.Client.TranslateText("ValueIsNotChanged") };
        }
        protected virtual object OnCellValueChanged(T item, Columns.BaseColumn<TModel, T> Column, string Value)
        {
            return new { success = 0, message = "" };
        }
        protected virtual void ProcessQuery()
        {
            var additionalParams = new Dictionary<string, object>();
            var isQueryableDataSet = false;
            if (this.DataSource.Query != null)
                isQueryableDataSet = this.DataSource.Query.GetType().IsQueryableDataSet();

            if (this.DataSource.RemoteDataSource != null && this.DataSource.Query == null && !this.DataSource.DataImportPreview && !this.ParentDrawsLayout)
            {
                this.DataSource.Query = this.DataSource.Items.AsQueryable();
            }
            if ((this.DataSource.RemoteDataSource != null || this.DataSource.Items == null || this.DataSource.Items.Count == 0) && this.DataSource.Query != null && !this.DataSource.DataImportPreview)
            {
                var defaultModel = Activator.CreateInstance(typeof(TModel));

                foreach (Ophelia.Web.View.Mvc.Controls.Binders.Fields.BaseField<TModel> item in this.FilterPanel.Controls)
                {
                    try
                    {
                        var path = "";

                        if (item.Expression != null)
                            path = item.Expression.ParsePath();
                        else
                        {
                            if (!string.IsNullOrEmpty(item.DataControl.ID))
                            {
                                path = item.DataControl.ID.Replace("_", ".");
                                if (item.DataControl.ID.IndexOf("Filters") == -1)
                                    path = "Filters." + path;
                            }
                            else if (!string.IsNullOrEmpty(item.Text))
                            {
                                path = item.Text.Replace("_", ".");
                                if (item.Text.IndexOf("Filters") == -1)
                                    path = "Filters." + path;
                            }
                            else if (item is Fields.DateField<TModel> dateFieldItem)
                            {
                                if (dateFieldItem.LowExpression != null)
                                    path = dateFieldItem.LowExpression.ParsePath().Replace("Low", "");
                                else if (dateFieldItem.HighExpression != null)
                                    path = dateFieldItem.HighExpression.ParsePath().Replace("High", "");
                            }
                        }
                        if (string.IsNullOrEmpty(path))
                            continue;

                        var entityProp = path.Replace("Filters.", "");
                        string value = "";
                        string lowValue = "";
                        string highValue = "";

                        var doubleSelection = false;
                        if (item is Fields.NumberboxField<TModel> && (item as Fields.NumberboxField<TModel>).Mode == Fields.NumberboxFieldMode.DoubleSelection)
                        {
                            doubleSelection = true;

                            var tmpPath = (item as Fields.NumberboxField<TModel>).LowPropertyName.Replace("_", ".");
                            if ((item as Fields.NumberboxField<TModel>).LowExpression == null || !string.IsNullOrEmpty(this.Request.GetValue(tmpPath)))
                                lowValue = this.Request.GetValue(tmpPath);
                            else
                                lowValue = Convert.ToString(item.GetExpressionValue((item as Fields.NumberboxField<TModel>).LowExpression));

                            tmpPath = (item as Fields.NumberboxField<TModel>).HighPropertyName.Replace("_", ".");
                            if ((item as Fields.NumberboxField<TModel>).HighExpression == null || !string.IsNullOrEmpty(this.Request.GetValue(tmpPath)))
                                highValue = this.Request.GetValue(tmpPath);
                            else
                                highValue = Convert.ToString(item.GetExpressionValue((item as Fields.NumberboxField<TModel>).HighExpression));
                        }
                        else if (item is Fields.DateField<TModel> && (item as Fields.DateField<TModel>).Mode == Fields.DateFieldMode.DoubleSelection)
                        {
                            doubleSelection = true;

                            var tmpPath = (item as Fields.DateField<TModel>).LowPropertyName.Replace("_", ".");
                            if ((item as Fields.DateField<TModel>).LowExpression == null || !string.IsNullOrEmpty(this.Request.GetValue(tmpPath)))
                                lowValue = this.Request.GetValue(tmpPath);
                            else
                                lowValue = Convert.ToString(item.GetExpressionValue((item as Fields.DateField<TModel>).LowExpression));

                            tmpPath = (item as Fields.DateField<TModel>).HighPropertyName.Replace("_", ".");
                            if ((item as Fields.DateField<TModel>).HighExpression == null || !string.IsNullOrEmpty(this.Request.GetValue(tmpPath)))
                                highValue = this.Request.GetValue(tmpPath);
                            else
                                highValue = Convert.ToString(item.GetExpressionValue((item as Fields.DateField<TModel>).HighExpression));
                        }
                        else
                        {
                            if (item.Expression == null || !string.IsNullOrEmpty(this.Request.GetValue(path)))
                                value = this.Request.GetValue(path);
                            else
                                value = Convert.ToString(item.GetExpressionValue());
                        }

                        object formattedValue = null;
                        var defaultValue = Convert.ToString(defaultModel.GetPropertyValue(path));
                        if ((doubleSelection && (!string.IsNullOrEmpty(lowValue) || !string.IsNullOrEmpty(highValue))) || (!string.IsNullOrEmpty(this.Request.GetValue(path)) && defaultValue != value && value != null))
                        {
                            var propTree = typeof(T).GetPropertyInfoTree(entityProp);
                            var propInfo = propTree.LastOrDefault();
                            var propType = propInfo?.PropertyType;
                            if (propType != null)
                            {
                                if (propType.IsGenericType && propType.Name.StartsWith("Null"))
                                    propType = propType.GenericTypeArguments[0];

                                if (propType.Name == "Boolean")
                                {
                                    if (value == "-1")
                                        continue;
                                    else
                                    {
                                        value = value == "1" ? "true" : "false";
                                        formattedValue = Convert.ChangeType(value, propType);
                                        if (isQueryableDataSet)
                                            this.DataSource.Query = (this.DataSource.Query as Ophelia.Data.Model.QueryableDataSet<T>).Where(propTree, formattedValue);
                                        else
                                            this.DataSource.Query = this.DataSource.Query.Where(entityProp + " = @0", formattedValue);
                                    }
                                }
                                else if (propType.Name.Contains("String"))
                                {
                                    formattedValue = Convert.ChangeType(value, propType);
                                    if (isQueryableDataSet)
                                        this.DataSource.Query = (this.DataSource.Query as Ophelia.Data.Model.QueryableDataSet<T>).Where(propTree, formattedValue, Comparison.Contains);
                                    else
                                        this.DataSource.Query = this.DataSource.Query.Where(entityProp + ".Contains(@0)", formattedValue);
                                }
                                else if (propType.IsNumeric())
                                {
                                    if (propType.Name == "Decimal")
                                    {
                                        var customCulture = (System.Globalization.CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
                                        if (customCulture.NumberFormat.NumberDecimalSeparator == ",")
                                        {
                                            if (!string.IsNullOrEmpty(value))
                                                value = value.Replace(".", ",");
                                            if (!string.IsNullOrEmpty(lowValue))
                                                lowValue = lowValue.Replace(".", ",");
                                            if (!string.IsNullOrEmpty(highValue))
                                                highValue = highValue.Replace(".", ",");
                                        }
                                        else if (customCulture.NumberFormat.NumberDecimalSeparator == ".")
                                        {
                                            if (!string.IsNullOrEmpty(value))
                                                value = value.Replace(",", ".");
                                            if (!string.IsNullOrEmpty(lowValue))
                                                lowValue = lowValue.Replace(",", ".");
                                            if (!string.IsNullOrEmpty(highValue))
                                                highValue = highValue.Replace(",", ".");
                                        }

                                        if (doubleSelection)
                                        {
                                            if (!string.IsNullOrEmpty(lowValue))
                                            {
                                                formattedValue = Convert.ChangeType(lowValue, propType);
                                                if (isQueryableDataSet)
                                                    this.DataSource.Query = (this.DataSource.Query as Ophelia.Data.Model.QueryableDataSet<T>).Where(propTree, formattedValue, Comparison.GreaterAndEqual);
                                                else
                                                    this.DataSource.Query = this.DataSource.Query.Where(entityProp + " >= @0", formattedValue);
                                            }
                                            if (!string.IsNullOrEmpty(highValue))
                                            {
                                                formattedValue = Convert.ChangeType(highValue, propType);
                                                if (isQueryableDataSet)
                                                    this.DataSource.Query = (this.DataSource.Query as Ophelia.Data.Model.QueryableDataSet<T>).Where(propTree, formattedValue, Comparison.LessAndEqual);
                                                else
                                                    this.DataSource.Query = this.DataSource.Query.Where(entityProp + " <= @0", formattedValue);
                                            }
                                        }
                                        else
                                        {
                                            formattedValue = Convert.ChangeType(value, propType);
                                            if (isQueryableDataSet)
                                                this.DataSource.Query = (this.DataSource.Query as Ophelia.Data.Model.QueryableDataSet<T>).Where(propTree, formattedValue);
                                            else
                                                this.DataSource.Query = this.DataSource.Query.Where(entityProp + " = @0", formattedValue);
                                        }
                                    }
                                    else
                                    {
                                        if (value.IndexOf(",") > -1)
                                        {
                                            var orParams = "";
                                            var parameters = new List<object>();
                                            var values = value.Split(',');
                                            var counter = 0;
                                            foreach (var val in values)
                                            {
                                                try
                                                {
                                                    parameters.Add(Convert.ChangeType(val, propType));
                                                    if (!string.IsNullOrEmpty(orParams))
                                                        orParams += " || ";
                                                    orParams += entityProp + " = @" + counter;
                                                }
                                                catch (Exception)
                                                {
                                                    counter++;
                                                    continue;
                                                }
                                                counter++;
                                            }
                                            if (parameters.Count > 0)
                                                if (isQueryableDataSet)
                                                    this.DataSource.Query = (this.DataSource.Query as Ophelia.Data.Model.QueryableDataSet<T>).Where(propTree, parameters.ToArray(), Comparison.In);
                                                else
                                                    this.DataSource.Query = this.DataSource.Query.Where(orParams, parameters.ToArray());
                                        }
                                        else
                                        {
                                            if (doubleSelection)
                                            {
                                                if (!string.IsNullOrEmpty(lowValue))
                                                {
                                                    formattedValue = Convert.ChangeType(lowValue, propType);
                                                    if (isQueryableDataSet)
                                                        this.DataSource.Query = (this.DataSource.Query as Ophelia.Data.Model.QueryableDataSet<T>).Where(propTree, formattedValue, Comparison.GreaterAndEqual);
                                                    else
                                                        this.DataSource.Query = this.DataSource.Query.Where(entityProp + " >= @0", formattedValue);
                                                }
                                                if (!string.IsNullOrEmpty(highValue))
                                                {
                                                    formattedValue = Convert.ChangeType(highValue, propType);
                                                    if (isQueryableDataSet)
                                                        this.DataSource.Query = (this.DataSource.Query as Ophelia.Data.Model.QueryableDataSet<T>).Where(propTree, formattedValue, Comparison.LessAndEqual);
                                                    else
                                                        this.DataSource.Query = this.DataSource.Query.Where(entityProp + " <= @0", formattedValue);
                                                }
                                            }
                                            else
                                            {
                                                formattedValue = Convert.ChangeType(value, propType);
                                                if (isQueryableDataSet)
                                                    this.DataSource.Query = (this.DataSource.Query as Ophelia.Data.Model.QueryableDataSet<T>).Where(propTree, formattedValue);
                                                else
                                                    this.DataSource.Query = this.DataSource.Query.Where(entityProp + " = @0", formattedValue);
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    if (doubleSelection)
                                    {
                                        if (!string.IsNullOrEmpty(lowValue))
                                        {
                                            formattedValue = Convert.ChangeType(lowValue, propType);
                                            if (isQueryableDataSet)
                                                this.DataSource.Query = (this.DataSource.Query as Ophelia.Data.Model.QueryableDataSet<T>).Where(propTree, formattedValue, Comparison.GreaterAndEqual);
                                            else
                                                this.DataSource.Query = this.DataSource.Query.Where(entityProp + " >= @0", formattedValue);
                                        }
                                        if (!string.IsNullOrEmpty(highValue))
                                        {
                                            formattedValue = Convert.ChangeType(highValue, propType);
                                            if (isQueryableDataSet)
                                                this.DataSource.Query = (this.DataSource.Query as Ophelia.Data.Model.QueryableDataSet<T>).Where(propTree, formattedValue, Comparison.LessAndEqual);
                                            else
                                                this.DataSource.Query = this.DataSource.Query.Where(entityProp + " <= @0", formattedValue);
                                        }
                                    }
                                    else
                                    {
                                        formattedValue = Convert.ChangeType(value, propType);
                                        if (isQueryableDataSet)
                                            this.DataSource.Query = (this.DataSource.Query as Ophelia.Data.Model.QueryableDataSet<T>).Where(propTree, formattedValue);
                                        else
                                            this.DataSource.Query = this.DataSource.Query.Where(entityProp + " = @0", formattedValue);
                                    }
                                }
                            }
                            else
                            {
                                additionalParams.Add(entityProp, value);
                            }

                            var modelProperty = typeof(TModel).GetPropertyInfo(path.Remove(path.Length - 2, 2));
                            if (path.EndsWith("ID") && modelProperty != null && (modelProperty.PropertyType.IsDataEntity() || modelProperty.PropertyType.IsPOCOEntity()))
                            {
                                var accessor = new Accessor();
                                accessor.Item = this.DataSource;
                                accessor.MemberName = path.Remove(path.Length - 2, 2);
                                accessor.Value = this.GetReferencedEntity(modelProperty.PropertyType, (formattedValue != null ? formattedValue : value));
                                accessor = null;
                            }
                        }
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                }
                defaultModel = null;
                if (!string.IsNullOrEmpty(this.Request.GetValue("OrderBy")))
                {
                    var direction = this.Request.GetValue("OrderByDirection")?.ToLower();
                    if (direction != "desc" && direction != "asc")
                        direction = "asc";

                    foreach (var column in this.Columns)
                    {
                        if (column.IsSortable && column.Expression?.ParsePath() == this.Request.GetValue("OrderBy"))
                        {
                            var propInfo = typeof(T).GetProperty(this.Request.GetValue("OrderBy"));
                            var ordering = "";
                            if (propInfo != null)
                            {
                                if (this.Request.GetValue("OrderBy").EndsWith("ID"))
                                {
                                    var entityPropInfo = typeof(T).GetProperty(this.Request.GetValue("OrderBy").Remove(this.Request.GetValue("OrderBy").IndexOf("ID"), 2));
                                    if (entityPropInfo != null)
                                    {
                                        ordering = entityPropInfo.Name;
                                        if (entityPropInfo.PropertyType.GetProperty("Name") != null)
                                            ordering += ".Name";
                                        else if (entityPropInfo.PropertyType.GetProperty("Title") != null)
                                            ordering += ".Title";
                                        else
                                            ordering = this.GetSortingFieldName(entityPropInfo);
                                    }
                                    else
                                        ordering = this.Request.GetValue("OrderBy");
                                }
                                else
                                {
                                    ordering = this.Request.GetValue("OrderBy");
                                }
                                if (ordering.Contains("()"))
                                {
                                    ordering = string.Join(".", ordering.Split('.').Take(ordering.Split('.').Length - 1));
                                    ordering += "." + typeof(T).GetDisplayTextProperty(ordering).Name;
                                }
                                if (isQueryableDataSet)
                                    this.DataSource.Query = Ophelia.Data.QueryableDataSetExtensions.OrderBy(this.DataSource.Query as Ophelia.Data.Model.QueryableDataSet<T>, ordering + " " + direction);
                                else
                                    this.DataSource.Query = this.DataSource.Query.OrderBy(ordering + " " + direction);
                            }
                        }
                    }
                }

                foreach (var grouper in this.Groupers)
                {
                    if (!grouper.IsSelected)
                        grouper.IsSelected = this.Request.GetValue(grouper.FormatRequestName()) == "on";
                }
                var selectedGroupers = this.Groupers.Where(op => op.IsSelected).Select(op => op.Expression).ToList();
                if (selectedGroupers.Count > 0)
                {
                    if (!string.IsNullOrEmpty(this.Request.GetValue(this.DataSource.GroupPagination.PageKey)) && this.Request.GetValue(this.DataSource.GroupPagination.PageKey).IsNumeric())
                        this.DataSource.GroupPagination.PageNumber = Convert.ToInt32(this.Request.GetValue(this.DataSource.GroupPagination.PageKey));

                    this.ContentRenderMode = ContentRenderMode.Group;

                    var groupedData = (IQueryable<IGrouping<object, T>>)this.DataSource.Query.GroupBy(selectedGroupers.ToArray());
                    if (this.DataSource.RemoteDataSource != null)
                    {
                        this.DataSource.GroupPagination.ItemCount = groupedData.Count();
                        this.GroupedData = groupedData.Paginate(this.CanExport ? 1 : this.DataSource.GroupPagination.PageNumber, this.CanExport ? int.MaxValue : this.DataSource.GroupPagination.PageSize);
                    }
                    else
                    {
                        this.DataSource.GroupPagination.ItemCount = groupedData.Count();
                        this.GroupedData = groupedData.Paginate(this.CanExport ? 1 : this.DataSource.GroupPagination.PageNumber, this.CanExport ? int.MaxValue : this.DataSource.GroupPagination.PageSize);
                    }
                    groupedData = null;
                    this.DataSource.Query = null;
                }
                else
                {
                    if (this.DataSource.RemoteDataSource != null && !this.DataSource.DataImportPreview && !this.DataSource.ParentDrawsLayout)
                    {
                        using (var queryData = new QueryData())
                        {
                            using (var visitor = new SQLPreparationVisitor(queryData))
                            {
                                if (this.DataSource.OnBeforeQueryExecuted != null)
                                    this.DataSource.Query = this.DataSource.OnBeforeQueryExecuted(this.DataSource.Query);

                                this.OnBeforeQueryExecuted();
                                visitor.Visit(this.DataSource.Query.Expression);
                                var request = new Service.WebApiCollectionRequest<T>() { Page = this.DataSource.Pagination.PageNumber, PageSize = this.DataSource.Pagination.PageSize, QueryData = queryData.Serialize(), Parameters = additionalParams, TypeName = typeof(T).FullName, Data = this.FiltersToEntity() };
                                if (this.DataSource.OnBeforeRemoteDataSourceCall != null)
                                    request = this.DataSource.OnBeforeRemoteDataSourceCall(request);

                                var response = this.DataSource.RemoteDataSource("Get" + typeof(T).Name.Pluralize(), request);
                                if (response.RawData != null)
                                    this.DataSource.Items = (List<T>)response.RawData;
                                else
                                    this.DataSource.Items = (List<T>)response.GetPropertyValue("Data");

                                this.DataSource.Pagination.ItemCount = response.TotalDataCount;

                                this.OnAfterQueryExecuted();
                            }
                        }
                    }
                    else
                    {
                        if (this.DataSource.OnBeforeQueryExecuted != null)
                            this.DataSource.Query = this.DataSource.OnBeforeQueryExecuted(this.DataSource.Query);

                        this.OnBeforeQueryExecuted();
                        if (this.DataSource.Query.GetType().IsQueryableDataSet())
                        {
                            this.DataSource.Items = Ophelia.Data.QueryableDataSetExtensions.Paginate(this.DataSource.Query as Ophelia.Data.Model.QueryableDataSet<T>, this.CanExport ? 1 : this.DataSource.Pagination.PageNumber, this.CanExport ? int.MaxValue : this.DataSource.Pagination.PageSize).ToList();
                        }
                        else
                        {
                            this.DataSource.Items = this.DataSource.Query.Paginate(this.CanExport ? 1 : this.DataSource.Pagination.PageNumber, this.CanExport ? int.MaxValue : this.DataSource.Pagination.PageSize).ToList();
                        }
                        this.DataSource.Pagination.ItemCount = this.DataSource.Query.Count();

                        this.OnAfterQueryExecuted();
                    }
                }
            }
        }
        protected virtual void OnAfterQueryExecuted()
        {

        }
        protected virtual void OnBeforeQueryExecuted()
        {

        }
        protected virtual string GetSortingFieldName(PropertyInfo info)
        {
            return info.Name + "ID";
        }

        protected virtual object GetReferencedEntity(Type entityType, object value)
        {
            return null;
        }
        protected virtual T FiltersToEntity()
        {
            try
            {
                var entity = (T)Activator.CreateInstance(typeof(T));
                var filters = this.DataSource.GetPropertyValue("Filters");
                var props = filters.GetType().GetProperties();
                foreach (var prop in props)
                {
                    var entityProp = entity.GetType().GetProperty(prop.Name);
                    try
                    {
                        if (entityProp == null)
                            continue;

                        entityProp.SetValue(entity, prop.GetValue(filters));
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                }
                return entity;
            }
            catch (Exception)
            {
                return null;
            }
        }
        public virtual void RenderHeader()
        {
            this.RenderBreadcrumb();
            this.Output.Write("<div class='table-container'>");
            this.RenderFilterPanel();
        }

        protected virtual void RenderFilterPanel()
        {

        }

        protected virtual void RenderPagination()
        {
            if (this.ContentRenderMode == ContentRenderMode.Group)
            {
                var paginator = new Paginator(this.Request, Convert.ToInt32(this.DataSource.GroupPagination.ItemCount), this.DataSource.GroupPagination.PageSize, this.DataSource.GroupPagination.PageNumber, this.DataSource.GroupPagination.LinkedPageCount, this.DataSource.GroupPagination.PageKey);
                paginator.ExcludedKeys = new string[] { "IsAjaxRequest", "AjaxEntityBinder", "isajaxrequest", "ajaxentitybinder" };
                paginator.StartTitle = this.Client.TranslateText("FirstPage");
                paginator.EndTitle = this.Client.TranslateText("LastPage");
                paginator.NextPageTitle = this.Client.TranslateText("NextPage");
                paginator.PreviousPageTitle = this.Client.TranslateText("PreviousPage");
                paginator.CssClass += " dataTables_paginate";
                paginator.RenderControlAsText(this.Output);
            }
            else if (this.ContentRenderMode == ContentRenderMode.Normal)
            {
                var paginator = new Paginator(this.Request, Convert.ToInt32(this.DataSource.Pagination.ItemCount), this.DataSource.Pagination.PageSize, this.DataSource.Pagination.PageNumber, this.DataSource.Pagination.LinkedPageCount, this.DataSource.Pagination.PageKey);
                paginator.ExcludedKeys = new string[] { "IsAjaxRequest", "AjaxEntityBinder", "isajaxrequest", "ajaxentitybinder" };
                paginator.StartTitle = this.Client.TranslateText("FirstPage");
                paginator.EndTitle = this.Client.TranslateText("LastPage");
                paginator.NextPageTitle = this.Client.TranslateText("NextPage");
                paginator.PreviousPageTitle = this.Client.TranslateText("PreviousPage");
                paginator.CssClass += " dataTables_paginate";
                paginator.RenderControlAsText(this.Output);
            }
        }
        protected string GetClassName()
        {
            return typeof(T).GetNamespace() + "." + typeof(T).Name;
        }
        public virtual void RenderContent()
        {
            //if (!this.ParentDrawsLayout)
            //{
            //    if (this.Messages != null && this.Messages.Count > 0)
            //    {
            //        var messageType = !this.Messages.Where(op => op.IsSuccess == true).Any() ? "warning" : "success";
            //        this.Output.Write("<div class=\"alert alert-" + messageType + " alert-styled-left\">");
            //        this.Output.Write("<button type=\"button\" class=\"close\" data-dismiss=\"alert\"><span>×</span><span class=\"sr-only\">" + this.Client.TranslateText("Close") + "</span></button>");
            //        this.Output.Write("<ul>");
            //        foreach (var message in this.Messages)
            //        {
            //            this.Output.Write("<li>");
            //            this.Output.Write(this.Client.TranslateText(message.Description));
            //            this.Output.Write("</li>");
            //        }
            //        this.Output.Write("</ul>");
            //        this.Output.Write("</div>"); /* alert */
            //    }
            //}
            if (this.DataSource != null && this.DataSource.Items != null && this.DataSource.Items.Count > 0 && this.ContentRenderMode == ContentRenderMode.Normal)
            {
                this.ReorderColumns();
                if (string.IsNullOrEmpty(this.Configuration.ContentTableCSSClass))
                    this.Configuration.ContentTableCSSClass = "table table-striped datatable-responsive-row-control" + (this.Configuration.AllowServerSideOrdering ? " disable-client-side-sorting" : "");
                else
                    this.Configuration.ContentTableCSSClass += (this.Configuration.AllowServerSideOrdering ? " disable-client-side-sorting" : "");

                this.Output.Write("<table class=\"" + this.Configuration.ContentTableCSSClass + (this.Configuration.EnableColumnFiltering ? " filterable" : "") + "\" column-filtering-type='" + this.Configuration.ColumnFilteringType.ToString() + "' data-class='" + GetClassName() + "' data-rowupdate='" + this.Configuration.RowUpdateType.ToString() + "'>");

                this.Output.Write("<thead>");
                this.Output.Write("<tr>");
                if (this.Configuration.AddBlankColumnToStart)
                    this.Output.Write("<th class='no-sort'></th>");
                if (this.Configuration.Checkboxes && this.Configuration.ShowCheckAll)
                {
                    this.Output.Write("<th class='no-sort'><input type='checkbox' id='CheckAll' class='binder-check-all' name='CheckAll'> " + this.Client.TranslateText("All") + "</th>");
                }
                else if (this.Configuration.Checkboxes)
                {
                    this.Output.Write("<th class='no-sort'></th>");
                }

                this.RenderOnBeforeDrawLine(null);
                foreach (var column in this.Columns)
                {
                    if (column.Visible)
                    {
                        if (this.Groupers.Where(op => op.IsSelected && (op.FormatName() == column.FormatName())).Any())
                            continue;

                        this.Output.Write("<th");
                        var className = "";
                        if (column.Alignment == HorizontalAlign.Right)
                            className += " text-right";

                        var link = "";
                        if (!this.ParentDrawsLayout && this.Configuration.AllowServerSideOrdering && column.IsSortable && this.GroupedData == null)
                        {
                            if (string.IsNullOrEmpty(column.Name))
                                column.Name = column.FormatName();

                            var qs = new Ophelia.Web.Application.Client.QueryString(this.Request);
                            qs.Update("OrderBy", column.Expression.ParsePath());
                            if (this.Request.GetValue("OrderBy") == column.Expression.ParsePath())
                            {
                                if (this.Request.GetValue("OrderByDirection") != null && this.Request.GetValue("OrderByDirection").Equals("ASC", StringComparison.InvariantCultureIgnoreCase))
                                {
                                    qs.Update("OrderByDirection", "DESC");
                                    this.Output.Write(" aria-sort='ascending' data-class='sorting_asc'");
                                    className += " sorting_asc";
                                }
                                else
                                {
                                    qs.Update("OrderByDirection", "ASC");
                                    this.Output.Write(" aria-sort='descending' data-class='sorting_desc'");
                                    className += " sorting_desc";
                                }
                            }
                            else
                            {
                                qs.Update("OrderByDirection", "ASC");
                                this.Output.Write(" aria-sort='ascending' data-class='sorting'");
                                className += " sorting";
                            }
                            className += " no-sort";
                            link = qs.Value;
                            if (!this.Configuration.ColumnSortingByLink)
                                this.Output.Write(" onclick=\"document.location.href='" + link + "'\"");
                        }
                        if (!string.IsNullOrEmpty(className))
                            this.Output.Write(" class='" + className + "'");
                        this.Output.Write(" data-name='" + column.FormatColumnName() + "'");
                        this.Output.Write(">");
                        if (!column.HideColumnTitle)
                        {
                            if (this.Configuration.ColumnSortingByLink)
                                this.Output.Write("<a class='column-header-link' href='" + link + "'>");
                            this.Output.Write(column.FormatText());
                            if (this.Configuration.ColumnSortingByLink)
                                this.Output.Write("</a>");
                        }
                        this.Output.Write("</th>");
                    }
                }
                this.RenderOnAfterDrawLine(null);
                this.Output.Write("</tr>");
                if (this.Configuration.ColumnFiltersInHead && !this.ParentDrawsLayout && this.GroupedData == null)
                    this.RenderColumnFilters();
                this.Output.Write("</thead>");
                this.Output.Write("<tbody>");
                if (!this.Configuration.ColumnFiltersInHead && !this.ParentDrawsLayout && this.GroupedData == null)
                    this.RenderColumnFilters();

                T blankItem = null;
                if (this.Configuration.AllowNewRow)
                {
                    blankItem = this.CreateNewItem();
                    if (blankItem != null)
                        this.DataSource.Items.Insert(0, blankItem);
                }
                var counter = -1;
                var allIDs = "";
                if (this.Configuration.AppendListOfIDOnItemLink)
                {
                    foreach (T item in this.DataSource.Items)
                    {
                        if (!string.IsNullOrEmpty(allIDs))
                            allIDs += ",";
                        allIDs += item.GetPropertyValue("ID");
                    }
                    allIDs = "IDList=" + allIDs;
                }
                foreach (T item in this.DataSource.Items)
                {
                    counter++;
                    var link = new Link();
                    try
                    {
                        link.URL = this.GetItemLink(item);
                    }
                    catch (Exception)
                    {
                        link.URL = "javascript:void(0);";
                    }
                    if (link.URL.IndexOf("?") == -1)
                        link.URL += "?";

                    link.URL = link.URL.Trim('&') + "&" + allIDs;

                    this.Output.Write("<tr ");
                    this.Output.Write("data-name=\"" + this.GetDisplayName(item) + "\" ");
                    this.RenderRowProperties(item, counter);
                    this.Output.Write(">");
                    if (this.Configuration.AddBlankColumnToStart)
                        this.Output.Write("<td></td>");
                    this.RenderOnBeforeDrawLine(item);
                    if (this.Configuration.Checkboxes && !string.IsNullOrEmpty(this.Configuration.CheckboxProperty) && item.GetType().GetProperties().Where(op => op.Name == this.Configuration.CheckboxProperty).Any())
                    {
                        this.Output.Write("<td>");
                        if (this.OnDrawCheckbox(item))
                        {
                            var id = this.Configuration.CheckboxProperty;
                            var identifier = "";
                            if (!string.IsNullOrEmpty(this.Configuration.CheckboxIdentifierProperty))
                            {
                                identifier = Convert.ToString(item.GetPropertyValue(this.Configuration.CheckboxIdentifierProperty));
                                id += "_" + identifier;
                            }
                            var val = Convert.ToBoolean(item.GetPropertyValue(this.Configuration.CheckboxProperty));

                            this.Output.Write("<input type='checkbox' class='binder-checkbox' name='" + this.Configuration.CheckboxProperty + "' id='" + id + "'" + (val ? " checked" : "") + " value='" + identifier + "' " + this.GetBinderCheckboxProperties(item) + ">");
                        }
                        this.Output.Write("</td>");
                    }
                    foreach (var column in this.Columns)
                    {
                        if (column.Visible)
                        {
                            if (this.Groupers.Where(op => op.IsSelected && (op.FormatName() == column.FormatName())).Any())
                                continue;

                            this.OnBeforeGetCellValue(item, column);
                            var value = column.GetValue(item);
                            link.Text = Convert.ToString(value).RemoveHTML();
                            if (column.MaxTextLength > 0)
                            {
                                if (!string.IsNullOrEmpty(link.Text) && link.Text.Length > column.MaxTextLength)
                                {
                                    link.Title = link.Text;
                                    link.Text = link.Text.Left(column.MaxTextLength) + "...";
                                }
                            }
                            this.Output.Write("<td ");
                            if (!string.IsNullOrEmpty(column.Width))
                                this.Output.Write("style='width:" + (column.Width.IndexOf("%") == -1 ? column.Width + "px" : column.Width) + "'");
                            if (column.Alignment == HorizontalAlign.Right)
                                this.Output.Write("class='text-right'");
                            this.RenderCellProperties(item, column, link);
                            this.Output.Write(">");
                            this.OnBeforeRenderCell(item, column);
                            if (column.AllowEdit)
                            {
                                using (var editControl = column.GetEditableControl(item, value))
                                {
                                    editControl.AddAttribute("data-filters", this.Request.QueryString.ToString().Replace("IsAjaxRequest=1", "").Replace("ajaxentitybinder=1", "").Trim('&'));
                                    this.Output.Write(editControl.Draw());
                                }
                            }
                            else
                                this.Output.Write(link.Draw());
                            this.OnAfterRenderCell(item, column);
                            this.Output.Write("</td>");
                        }
                    }
                    this.RenderOnAfterDrawLine(item);
                    this.Output.Write("</tr>");
                }
                this.Output.Write("</tbody>");
                this.Output.Write("</table>");
            }
            else if (this.DataSource != null && this.GroupedData != null && this.ContentRenderMode == ContentRenderMode.Group)
            {
                var index = 0;
                var list = new List<IGrouping<object, T>>();
                foreach (var group in this.GroupedData)
                {
                    list.Add(group);
                }
                foreach (var group in list)
                {
                    this.DataSource.Pagination.PageKey = "grouppage" + index;
                    this.DataSource.Pagination.PageNumber = this.Request.GetValue(this.DataSource.Pagination.PageKey).ToInt32() > 0 ? this.Request.GetValue(this.DataSource.Pagination.PageKey).ToInt32() : 1;
                    this.DataSource.Pagination.ItemCount = group.Count();
                    this.DataSource.Items = group.Paginate(this.DataSource.Pagination.PageNumber, this.DataSource.Pagination.PageSize).ToList();

                    this.Output.Write("<div class='grouped-data col-sm-12 col-xs-12' id='grouped-data" + index + "'>");
                    this.Output.Write("<div class='group-header'>");
                    this.Output.Write("<a data-toggle='collapse' data-parent='#grouped-data" + index + "' href='#data" + index + "' onclick='return false;' class='" + (this.DataSource.Pagination.PageNumber == 1 ? "collapsed" : "") + "'>");
                    foreach (var grouper in this.Groupers)
                    {
                        if (grouper.IsSelected)
                        {
                            var name = grouper.Expression.ParsePath();
                            if (grouper.Expression.Body.Type.IsClass && !grouper.Expression.Body.Type.FullName.Contains("System."))
                                name += "ID";

                            if (name.IndexOf(".") > -1)
                                name = name.Replace(".", "");

                            var text = Convert.ToString((group.Key as Ophelia.DynamicClass).GetPropertyValue(name));

                            if (grouper.Type != null && grouper.Type.IsEnum)
                                text = grouper.Type.GetEnumDisplayName(text, this.Client);
                            else if (grouper.Expression.Body.Type.IsClass && !grouper.Expression.Body.Type.FullName.Contains("System.") && text.IsNumeric() && grouper.DisplayMemberExpression != null)
                            {
                                var refEntity = this.GetReferencedEntity(grouper.Expression.Body.Type, Convert.ToInt64(text));
                                if (refEntity != null)
                                {
                                    this.DataSource.Items.ForEach(op => op.SetPropertyValue(grouper.Expression.ParsePath(), refEntity));
                                    if (grouper.DisplayMemberExpression != null)
                                        text = Convert.ToString(grouper.DisplayMemberExpression.Execute(this.DataSource.Items[0]));
                                }
                            }
                            this.Output.Write("<label class='grouper-title'><strong>" + grouper.FormatText(this.Client) + ": </strong>" + text + "<label class='group-count'>" + this.Client.TranslateText("Count") + ":" + group.Count() + "</label></label>");
                        }
                    }
                    this.Output.Write("</a>");
                    this.Output.Write("</div>");

                    this.Output.Write("<div class='data collapse " + (this.DataSource.Pagination.PageNumber > 1 ? "in" : "") + "' id='data" + index + "'>");
                    this.ContentRenderMode = ContentRenderMode.Normal;
                    this.RenderContent();
                    this.ContentRenderMode = ContentRenderMode.Group;
                    this.Output.Write("</div>");

                    this.Output.Write("</div>");
                    index++;
                }
                list = null;
            }
            else
            {
                this.Output.Write("<div class=\"alert alert-info alert-styled-left alert-bordered empty-table-warning\">" + this.Client.TranslateText("ThereIsNoRecordToDisplay") + "</div>");
            }
        }
        protected virtual string GetItemLink(T item)
        {
            if (this.Configuration.DrawViewLinkInsteadOfEdit)
                return this.Configuration.ViewURL.TrimEnd('/') + "/" + item.GetPropertyValue("ID");
            else
            {
                var StartIndex = this.Configuration.EditURL.TrimEnd('/').IndexOf('?');
                if (StartIndex > -1)
                    return (this.Configuration.EditURL.TrimEnd('/')).ToString().Insert(StartIndex, item.GetPropertyValue("ID").ToString());
                else
                    return this.Configuration.EditURL.TrimEnd('/') + "/" + item.GetPropertyValue("ID").ToString();
            }
        }
        protected virtual string GetDisplayName(T entity)
        {
            var name = entity.GetPropertyValue("Name");
            if (name == null)
                name = entity.GetPropertyValue("Title");
            if (name != null)
                return name.ToString();
            return "";
        }
        protected virtual string GetBinderCheckboxProperties(T entity)
        {
            return "";
        }
        protected virtual T CreateNewItem()
        {
            return (T)Activator.CreateInstance(typeof(T));
        }
        protected virtual bool OnDrawCheckbox(T item)
        {
            return true;
        }
        protected virtual void RenderColumnFilters()
        {
            if (this.Configuration.EnableColumnFiltering && this.GroupedData == null)
            {
                var tag = "th";
                if (!this.Configuration.ColumnFiltersInHead)
                    tag = "td";
                this.Output.Write("<tr class='filters'>");
                if (this.Configuration.AddBlankColumnToStart)
                    this.Output.Write("<" + tag + " class='no-sort'></" + tag + ">");

                if (this.Configuration.Checkboxes)
                    this.Output.Write("<" + tag + " class='no-sort'></" + tag + ">");

                foreach (var column in this.Columns)
                {
                    if (column.Visible)
                    {
                        var columnName = column.FormatColumnName();
                        if (this.Groupers.Where(op => op.IsSelected && (op.FormatName() == columnName)).Any())
                            continue;

                        this.Output.Write("<" + tag + ">");
                        if (column.EnableColumnFiltering)
                        {
                            var includedToFilters = false;
                            foreach (Binders.Fields.BaseField<TModel> item in this.FilterPanel.Controls)
                            {
                                var path = "";
                                if (item.Expression != null)
                                {
                                    path = item.Expression.ParsePath();
                                }
                                else if (!string.IsNullOrEmpty(item.DataControl.ID))
                                {
                                    path = item.DataControl.ID.Replace("_", ".");
                                    if (item.DataControl.ID.IndexOf("Filters") == -1)
                                        path = "Filters." + path;
                                }
                                else if (!string.IsNullOrEmpty(item.Text))
                                {
                                    path = item.Text.Replace("_", ".");
                                    if (item.Text.IndexOf("Filters") == -1)
                                        path = "Filters." + path;
                                }

                                includedToFilters = path.Replace("Filters.", "").Replace("Low", "") == columnName;
                                if (includedToFilters)
                                    break;
                            }
                            if (includedToFilters)
                            {
                                if (column is Columns.BoolColumn<TModel, T>)
                                {
                                    this.Output.Write((column as Columns.BoolColumn<TModel, T>).GetEditableControlAsSelect(null, null).Draw());
                                }
                                else
                                {
                                    if (column is Columns.DateColumn<TModel, T>)
                                    {
                                        (column as Columns.DateColumn<TModel, T>).Mode = Fields.DateFieldMode.DoubleSelection;
                                    }
                                    this.Output.Write(column.GetEditableControl(null, null).Draw());
                                }
                            }
                        }
                        this.Output.Write("</" + tag + ">");
                    }
                }
                this.Output.Write("</tr>");
            }
        }
        protected virtual void Export()
        {
            if (this.CanExport)
            {
                this.ReorderColumns();
                this.ContentRenderMode = ContentRenderMode.Export;
                var fileName = "";
                if (!string.IsNullOrEmpty(ExportDocumentName))
                    fileName = ExportDocumentName;
                else if (typeof(T).Name.EndsWith("List"))
                    fileName = "ListOf" + typeof(T).Name;
                else
                    fileName = typeof(T).Name + "List";

                var grid = new Ophelia.Data.Exporter.Controls.Grid();
                grid.Text = this.Client.TranslateText(fileName);
                foreach (var column in this.Columns)
                {
                    if (column.Visible)
                    {
                        if (string.IsNullOrEmpty(column.Name))
                            column.Name = column.FormatName();

                        grid.Columns.Add(new Data.Exporter.Controls.Column(grid) { Text = column.FormatText(), ID = column.Name, IsNumeric = column.Alignment == HorizontalAlign.Right });
                    }
                }
                foreach (T item in this.DataSource.Items)
                {
                    var gridRow = new Ophelia.Data.Exporter.Controls.Row(grid);
                    Ophelia.Data.Exporter.Controls.Column gridColumn = null;
                    grid.Rows.Add(gridRow);
                    foreach (var column in this.Columns)
                    {
                        if (!column.Visible)
                            continue;

                        gridColumn = grid.Columns.Where(op => op.ID == column.Name).FirstOrDefault();

                        var cell = new Ophelia.Data.Exporter.Controls.Cell(grid, gridColumn, gridRow);
                        gridRow.Cells.Add(cell);

                        this.OnBeforeGetCellValue(item, column);
                        var value = column.GetValue(item);
                        value = this.FindCustomCellValue(item, column, value);
                        if (this.CanExportCellValue(column, value))
                        {
                            cell.Value = this.FormatCellValueForExport(item, column, value);
                            var link = new Ophelia.Web.UI.Controls.Link() { Text = Convert.ToString(cell.Value) };
                            this.RenderCellProperties(item, column, link);
                            cell.Value = link.Text;
                        }

                        value = null;
                    }
                }

                byte[] fileContent = null;
                var fileExtension = "xls";
                switch (this.ExportOutputFileFormat)
                {
                    case "xls":
                        fileExtension = "xlsx";
                        fileContent = new Ophelia.Data.Exporter.ExcelExporter().Export(grid);
                        break;
                    case "csv":
                        fileExtension = "csv";
                        fileContent = new Ophelia.Data.Exporter.CSVExporter().Export(grid);
                        break;
                    case "xml":
                        fileExtension = "xml";
                        fileContent = new Ophelia.Data.Exporter.XMLExporter().Export(grid);
                        break;
                    case "tdf":
                        fileExtension = "txt";
                        fileContent = new Ophelia.Data.Exporter.TDFExporter().Export(grid);
                        break;
                }
                grid.Dispose();
                grid = null;
                this.DataSource.Dispose();

                if (fileContent != null)
                {
                    this.Response.Clear();
                    this.Response.ClearContent();
                    this.Response.ClearHeaders();

                    this.Response.Headers["Content-Encoding"] = "windows-1254";
                    this.Response.Headers["charset"] = "windows-1254";

                    if (fileExtension.Equals("xml"))
                        this.Response.ContentType = "text/xml";
                    else if (fileExtension.Equals("txt"))
                        this.Response.ContentType = "text/plain";
                    else if (fileExtension.Equals("pdf"))
                        this.Response.ContentType = "application/pdf";
                    else if (fileExtension.Equals("xlsx") || fileExtension.Equals("csv"))
                        this.Response.ContentType = "application/vnd.ms-excel";
                    else
                        this.Response.ContentType = "application/octet-stream";

                    this.Response.AddHeader("Content-Disposition", "attachment; filename=" + this.Client.TranslateText(fileName).ReplaceSpecialVowelsAndConsonant() + "." + fileExtension);
                    if (fileContent != null)
                        this.Response.Write(fileContent);
                }
                this.Response.Flush();
                this.Response.End();
            }
        }
        protected virtual bool CanExportCellValue(Columns.BaseColumn<TModel, T> column, object value)
        {
            return true;
        }
        protected virtual object FormatCellValueForExport(T item, Columns.BaseColumn<TModel, T> column, object value)
        {
            return value;
        }
        protected virtual void OnBeforeGetCellValue(T item, Columns.BaseColumn<TModel, T> column)
        {

        }
        protected virtual object FindCustomCellValue(T item, Columns.BaseColumn<TModel, T> column, object value)
        {
            return value;
        }
        protected virtual void RenderOnBeforeDrawLine(T item)
        {

        }
        protected virtual void RenderOnAfterDrawLine(T item)
        {

        }
        protected virtual void RenderRowProperties(T item, int index)
        {
            this.Output.Write("class='" + ((index + 1) % 2 == 0 ? "even" : "odd") + "' ");
        }
        protected virtual void RenderCellProperties(T item, Columns.BaseColumn<TModel, T> column, Link link)
        {
            if (this.DataSource.DataImportPreview)
            {
                link.URL = "javascript:void(0)";
            }
        }
        protected virtual void OnBeforeRenderCell(T item, Columns.BaseColumn<TModel, T> column)
        {

        }
        protected virtual void OnAfterRenderCell(T item, Columns.BaseColumn<TModel, T> column)
        {

        }
        public virtual void RenderFooter()
        {
            this.Output.Write("</div>");//Table container
            this.Output.Write("<div class='datatable-footer collection-binder-foorer'>");
            this.RenderPagination();
            this.Output.Write("</div>");
        }
        public virtual void RenderBreadcrumb()
        {

        }

        protected virtual void Configure()
        {

        }
        protected virtual void onViewContextSet()
        {

        }
        protected virtual void ReorderColumns()
        {
            this.Columns = this.Columns.OrderBy(op => op.SortOrder).ToList();
        }
        public virtual bool CanAddColumn(Columns.BaseColumn<TModel, T> column)
        {
            return true;
        }
        protected virtual void FinishRequest()
        {
            if (this.Output != null)
            {
                this.Response.Write(this.Output);
            }
            this.Response.Flush();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }
    public enum ContentRenderMode
    {
        Group,
        Normal,
        Export
    }
}
