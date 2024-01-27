namespace Griddly.Mvc;

public abstract class GriddlySettings : IGriddlyFilterSettings
{
    public static GriddlyCss DefaultCss = GriddlyCss.Bootstrap3Defaults;

    #region Obsolete shims retained only for backward compatibility
    [Obsolete("Use DefaultCss.GriddlyDefault")]
    public static string DefaultClassName { get => DefaultCss.GriddlyDefault; set => DefaultCss.GriddlyDefault = value; }
    [Obsolete("Use DefaultCss.TableDefault")]
    public static string DefaultTableClassName { get => DefaultCss.TableDefault; set => DefaultCss.TableDefault = value; }
    [Obsolete("Use DefaultCss.ButtonDefault")]
    public static string DefaultButtonClassName { get => DefaultCss.ButtonDefault; set => DefaultCss.ButtonDefault = value; }
    [Obsolete("Use DefaultCss.IsBootstrap4")]
    public static bool IsBootstrap4 => DefaultCss.IsBootstrap4;
    #endregion

#if NETFRAMEWORK
    public static string ButtonTemplate = "~/Views/Shared/Griddly/BootstrapButton.cshtml";
    public static string ButtonListTemplate = "~/Views/Shared/Griddly/ButtonStrip.cshtml";
#else
    public static string ButtonTemplate = "~/Pages/Shared/Griddly/BootstrapButton.cshtml";
    public static string ButtonListTemplate = "~/Pages/Shared/Griddly/ButtonStrip.cshtml";
#endif

    public static HtmlString DefaultBoolTrueHtml = null;
    public static HtmlString DefaultBoolFalseHtml = null;
    public static string ColumnLinkTemplate { get; set; } = "<a href=\"{0}\">{1}</a>";
    public static int? DefaultPageSize = null;
    public static FilterMode? DefaultInitialFilterMode = FilterMode.Form;
    //public static FilterMode? DefaultAllowedFilterModes = FilterMode.Inline;
    public static bool DefaultShowRowSelectCount = true;
    public static bool ExportCurrencySymbol = true;
    public static bool DisableHistoryParameters = false;

    public static Func<string, string> DecorateCookieName = null;
    public static Func<GriddlyButton, object> IconTemplate = null;
    public static Func<GriddlyResultPage, object> DefaultFooterTemplate = null;
    public static Func<GriddlyResultPage, object> DefaultHeaderTemplate = null;
#if NETFRAMEWORK
        public static Func<IGriddlyFilterSettings, object> DefaultFilterModalHeaderTemplate = null;
        public static Func<IGriddlyFilterSettings, object> DefaultFilterModalFooterTemplate = null;
#else
        public static Func<IGriddlyFilterSettings, IHtmlContent> DefaultFilterModalHeaderTemplate = null;
        public static Func<IGriddlyFilterSettings, IHtmlContent> DefaultFilterModalFooterTemplate = null;

#endif
    public static Func<IEnumerable, GriddlySettings, IEnumerable> OnGriddlyExportExecuting = null;
    public static IGriddlyColumnValueFilter ColumnValueFilter = null;

#if NETFRAMEWORK
    /// <summary>
    /// Defines an event handler for custom export requests.
    /// 
    /// First argument is the record set. Second argument is the posted form values.
    /// </summary>
    [Obsolete("Use GlobalHandleCustomExport")]
    public static Func<GriddlyResult, NameValueCollection, ControllerContext, ActionResult> HandleCustomExport = null;
    [Obsolete("Use GlobalBeforeRender")]
    public static Action<GriddlySettings, GriddlyResultPage, HtmlHelper, bool> OnBeforeRender = null;
    [Obsolete("Use GlobalGriddlyResultExecuting")]
    public static Action<GriddlySettings, ControllerContext> OnGriddlyResultExecuting = null;
    [Obsolete("Use GlobalGriddlyPageExecuting")]
    public static Action<GriddlySettings, GriddlyContext, ControllerContext> OnGriddlyPageExecuting = null;
#else
    /// <summary>
    /// Defines an event handler for custom export requests.
    /// 
    /// First argument is the record set. Second argument is the posted form values.
    /// </summary>
    [Obsolete("Use GlobalHandleCustomExport")]
    public static Func<GriddlyResult, NameValueCollection, ActionContext, ActionResult> HandleCustomExport = null;
    [Obsolete("Use GlobalBeforeRender")]
    public static Action<GriddlySettings, GriddlyResultPage, IHtmlHelper, bool> OnBeforeRender = null;
    [Obsolete("Use GlobalGriddlyResultExecuting")]
    public static Action<GriddlySettings, ActionContext> OnGriddlyResultExecuting = null;
    [Obsolete("Use GlobalGriddlyPageExecuting")]
    public static Action<GriddlySettings, GriddlyContext, ActionContext> OnGriddlyPageExecuting = null;
#endif
    public static Func<HandleCustomExportArgs, ActionResult> GlobalHandleCustomExport = null;
    public static Action<BeforeRenderArgs> GlobalBeforeRender = null;
    public static Action<GriddlyResultExecutingArgs> GlobalGriddlyResultExecuting = null;
    public static Action<GriddlyPageExecutingArgs> GlobalGriddlyPageExecuting = null;

#if NETFRAMEWORK
    public GriddlySettings()
#else
    public GriddlySettings(IHtmlHelper html)
#endif

    {
#if NETFRAMEWORK
        IdProperty = "Id";
#endif

        Columns = new List<GriddlyColumn>();
        Filters = new List<GriddlyFilter>();
        Buttons = new List<GriddlyButton>();
        Exports = new List<GriddlyExport>();
        RowIds = new Dictionary<string, Func<object, object>>();
        HtmlAttributes = new RouteValueDictionary();
        TableHtmlAttributes = new RouteValueDictionary();

        ClassName = DefaultCss.GriddlyDefault;
        TableClassName = DefaultCss.TableDefault;
        FooterTemplate = DefaultFooterTemplate;
        HeaderTemplate = DefaultHeaderTemplate;
            FilterModalHeaderTemplate = DefaultFilterModalHeaderTemplate;
            FilterModalFooterTemplate = DefaultFilterModalFooterTemplate;
        EmptyGridMessageTemplate = DefaultEmptyGridMessageTemplate;
        EmptyGridMessage = DefaultEmptyGridMessage;
        PageSize = DefaultPageSize;
        InitialFilterMode = DefaultInitialFilterMode;
        //AllowedFilterModes = DefaultAllowedFilterModes;
        ShowRowSelectCount = DefaultShowRowSelectCount;

#if NETCOREAPP
        Html = html;
#endif
    }

    public static void ConfigureBootstrap4Defaults()
    {
        DefaultCss = GriddlyCss.Bootstrap4Defaults;
    }

    public GriddlyCss Css = DefaultCss;
    public HtmlString BoolTrueHtml = DefaultBoolTrueHtml;
    public HtmlString BoolFalseHtml = DefaultBoolFalseHtml;

#if NETCOREAPP
    public IHtmlHelper Html { get; set; }
#endif
    public string[] DefaultRowIds { get; set; }
#if NETFRAMEWORK
    public string IdProperty { get; set; }
#else
    public abstract object TryGetId(object row);
#endif
    public string Title { get; set; }
    public string ClassName { get; set; }
    public string TableClassName { get; set; }
    public FilterMode? AllowedFilterModes { get; set; }
    public FilterMode? InitialFilterMode { get; set; }
    public bool IsFilterFormInline { get; set; }
    public bool ShowRowSelectCount { get; set; }
    public IDictionary<string, object> HtmlAttributes { get; set; }
    public IDictionary<string, object> TableHtmlAttributes { get; set; }

    public int? PageSize { get; set; }
    public int? MaxPageSize { get; set; }

    public List<GriddlyColumn> Columns { get; set; }
    public List<GriddlyFilter> Filters { get; set; }
    public List<GriddlyButton> Buttons { get; set; }
    public List<GriddlyExport> Exports { get; set; }

#if NETFRAMEWORK
    public Action<GriddlySettings, GriddlyResultPage, HtmlHelper, bool> BeforeRender = null;
    public Func<object, object> BeforeTemplate { get; set; }
    public Func<object, object> AfterButtonsTemplate { get; set; }
    public Func<object, object> AfterTemplate { get; set; }
    public Func<GriddlySettings, object> FilterTemplate { get; set; }
    public Func<GriddlySettings, object> InlineFilterTemplate { get; set; }
        public Func<IGriddlyFilterSettings, object> FilterModalHeaderTemplate { get; set; }
        public Func<IGriddlyFilterSettings, object> FilterModalFooterTemplate { get; set; }
    public Func<object, object> FilterButtonTemplate { get; set; }
    public Func<object, object> BeforeColumnHeadersTemplate { get; set; }
#else
    public Action<GriddlySettings, GriddlyResultPage, IHtmlHelper, bool> BeforeRender = null;
    public Func<object, IHtmlContent> BeforeTemplate { get; set; }
    public Func<object, IHtmlContent> AfterButtonsTemplate { get; set; }
    public Func<object, IHtmlContent> AfterTemplate { get; set; }
    public Func<GriddlySettings, IHtmlContent> FilterTemplate { get; set; }
    public Func<GriddlySettings, IHtmlContent> InlineFilterTemplate { get; set; }
        public Func<IGriddlyFilterSettings, IHtmlContent> FilterModalHeaderTemplate { get; set; }
        public Func<IGriddlyFilterSettings, IHtmlContent> FilterModalFooterTemplate { get; set; }
    public Func<object, IHtmlContent> FilterButtonTemplate { get; set; }
    public Func<object, IHtmlContent> BeforeColumnHeadersTemplate { get; set; }
#endif

    public Func<object, object> RowClickUrl { get; set; }

    /// <summary>
    /// The anchor tag target for the <seealso cref="RowClickUrl"/>
    /// </summary>
    public string RowClickTarget { get; set; }
    public string RowClickModal { get; set; }
    public Func<object, object> RowClass { get; set; }
    public Func<object, object> RowHtmlAttributes { get; set; }

    public Func<GriddlyResultPage, object> FooterTemplate { get; set; }
    public Func<GriddlyResultPage, object> HeaderTemplate { get; set; }
    public Func<GriddlyResultPage, object> TitleAppend { get; set; }

    public static Func<EmptyGridMessageTemplateParams, object> DefaultEmptyGridMessageTemplate { get; set; }
    public Func<EmptyGridMessageTemplateParams, object> EmptyGridMessageTemplate { get; set; }
    public static string DefaultEmptyGridMessage { get; set; }
    public string EmptyGridMessage { get; set; }

    public Dictionary<string, Func<object, object>> RowIds { get; protected set; }

    public virtual bool HasRowClickUrl
    {
        get { return RowClickUrl != null; }
    }

    public virtual bool HasRowClickTarget
    {
        get { return !string.IsNullOrWhiteSpace(RowClickTarget); }
    }

    public virtual bool HasRowClass
    {
        get { return RowClass != null; }
    }

    public virtual object RenderRowClickUrl(object o)
    {
        return RowClickUrl(o);
    }

    public virtual object RenderRowClass(object o)
    {
        return RowClass(o);
    }

    public virtual IDictionary<string, object> GenerateRowHtmlAttributes(object o)
    {
        if (RowHtmlAttributes != null)
        {
            object value = RowHtmlAttributes(o);

            if (value != null)
            {
                RouteValueDictionary attributes = new RouteValueDictionary();

                if (!(value is IDictionary<string, object>))
                    value = HtmlHelper.AnonymousObjectToHtmlAttributes(value);

                foreach (KeyValuePair<string, object> entry in (IDictionary<string, object>)value)
                    attributes.Add(entry.Key, entry.Value);

                return attributes;
            }
        }

        return null;
    }

#if NETFRAMEWORK
    public GriddlySettings RowId(Expression<Func<object, object>> expression, string name = null)
#else
    public GriddlySettings RowId(Expression<Func<object, object>> expression, string name)
#endif
    {
        if (name == null)
        {
#if NETFRAMEWORK
            var meta = ModelMetadata.FromLambdaExpression(expression, new ViewDataDictionary<object>());
            name = ExpressionHelper.GetExpressionText(expression);
#else
            throw new ArgumentNullException("name", "The 'name' parameter is required on the non-generic RowId() method");
#endif
        }

        RowIds[name ?? "id"] = expression.Compile();

        return this;
    }

    public GriddlySettings Add(GriddlyColumn column, Func<GriddlyColumn, GriddlyFilter> filter = null)
    {
        if (filter != null)
        {
            GriddlyFilter filterDef = filter(column);

            if (filterDef != null)
            {
                column.Filter = filterDef;

                Filters.Add(filterDef);
            }
        }

        Columns.Add(column);

        return this;
    }

    public GriddlySettings Add(GriddlyButton button)
    {
        Buttons.Add(button);

        return this;
    }

    public GriddlySettings Add(GriddlyExport export)
    {
        Exports.Add(export);

        return this;
    }

    public GriddlySettings FilterBox(string field, string caption, FilterDataType dataType = FilterDataType.Decimal, string htmlClass = null, string captionPlural = null, string group = null, object inputHtmlAttributes = null)
    {
        return Add(GriddlyFilterExtensions.FilterBox(null, dataType, field, caption, htmlClass, captionPlural, group, inputHtmlAttributes: inputHtmlAttributes));
    }

    public GriddlySettings FilterRange(string field, string fieldEnd, string caption, FilterDataType dataType = FilterDataType.Decimal, string htmlClass = null, string captionPlural = null, string group = null, object inputHtmlAttributes = null)
    {
        return Add(GriddlyFilterExtensions.FilterRange(null, dataType, field, fieldEnd, caption, htmlClass, captionPlural, group, inputHtmlAttributes: inputHtmlAttributes));
    }

    public GriddlySettings FilterList(string field, string caption, IEnumerable<SelectListItem> items, bool isMultiple = true, bool defaultSelectAll = false, string nullItemText = null, bool isNoneAll = true, string htmlClass = null, string captionPlural = null, bool displayIncludeCaption = false, string group = null, object inputHtmlAttributes = null)
    {
        return Add(GriddlyFilterExtensions.FilterList(null, items, isMultiple, defaultSelectAll, nullItemText, isNoneAll, field, caption, htmlClass, captionPlural, displayIncludeCaption, group, inputHtmlAttributes: inputHtmlAttributes));
    }

    public GriddlySettings FilterEnum<T>(string field, string caption, bool isMultiple = true, bool defaultSelectAll = false, string nullItemText = null, bool isNoneAll = true, string htmlClass = null, string captionPlural = null, bool displayIncludeCaption = false, string group = null, object inputHtmlAttributes = null)
        where T : struct
    {
        return Add(GriddlyFilterExtensions.FilterEnum<T>(null, isMultiple, defaultSelectAll, nullItemText, isNoneAll, field, caption, htmlClass, captionPlural, displayIncludeCaption, group, inputHtmlAttributes: inputHtmlAttributes));
    }

    public GriddlySettings FilterEnum<T>(string field, string caption, IEnumerable<T> items, bool isMultiple = true, bool defaultSelectAll = false, string nullItemText = null, bool isNoneAll = true, string htmlClass = null, string captionPlural = null, bool displayIncludeCaption = false, string group = null, bool sort = true, object inputHtmlAttributes = null)
        where T : struct
    {
        return Add(GriddlyFilterExtensions.FilterEnum<T>(null, items, isMultiple, defaultSelectAll, nullItemText, isNoneAll, field, caption, htmlClass, captionPlural, displayIncludeCaption, group, sort: sort, inputHtmlAttributes: inputHtmlAttributes));
    }

    public GriddlySettings FilterBool(string field, string caption, string trueLabel = "Yes", string falseLabel = "No", string nullItemText = null, bool isMultiple = false, bool defaultSelectAll = false, bool isNoneAll = false, string htmlClass = null, string captionPlural = null, bool displayIncludeCaption = true, string group = null, object inputHtmlAttributes = null)
    {
        return Add(GriddlyFilterExtensions.FilterBool(null, trueLabel, falseLabel, nullItemText, isMultiple, defaultSelectAll, isNoneAll, field, caption, htmlClass, captionPlural, displayIncludeCaption, group, inputHtmlAttributes: inputHtmlAttributes));
    }

    public GriddlySettings Add(GriddlyFilter filter)
    {
        Filters.Add(filter);

        return this;
    }

    /*public GriddlySettings Button(string url, string caption, string icon = null)
    {
        return Add(new GriddlyButton()
            {
                HRef = url,
                Text = caption,
                Icon = icon
            });
    }*/

    public GriddlySettings Button(Func<object, object> argumentTemplate, string caption, string icon = null, GriddlyButtonAction action = GriddlyButtonAction.Navigate, bool? enableOnSelection = null, string className = null, string target = null, string[] rowIds = null, object htmlAttributes = null, bool appendRowIdsToUrl = false, string confirmMessage = null)
    {
        if (enableOnSelection == null)
            enableOnSelection = (action == GriddlyButtonAction.Ajax || action == GriddlyButtonAction.AjaxBulk || action == GriddlyButtonAction.Post);

        var button = new GriddlyButton(className)
        {
            ArgumentTemplate = argumentTemplate,
            Text = caption,
            Icon = icon,
            Action = action,
            EnableOnSelection = enableOnSelection.Value,
            Target = target,
            RowIds = rowIds,
            AppendRowIdsToUrl = appendRowIdsToUrl,
            ConfirmMessage = confirmMessage
        };

        if (htmlAttributes != null)
            button.HtmlAttributes = HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes);

        return Add(button);
    }

    public GriddlySettings Button(string argument, string caption, string icon = null, GriddlyButtonAction action = GriddlyButtonAction.Navigate, bool? enableOnSelection = null, string className = null, string target = null, string[] rowIds = null, object htmlAttributes = null, bool appendRowIdsToUrl = false, string confirmMessage = null)
    {
        if (enableOnSelection == null)
            enableOnSelection = (action == GriddlyButtonAction.Ajax || action == GriddlyButtonAction.AjaxBulk || action == GriddlyButtonAction.Post);

        var button = new GriddlyButton(className)
        {
            Argument = argument,
            Text = caption,
            Icon = icon,
            Action = action,
            EnableOnSelection = enableOnSelection.Value,
            Target = target,
            RowIds = rowIds,
            AppendRowIdsToUrl = appendRowIdsToUrl,
            ConfirmMessage = confirmMessage
        };

        if (htmlAttributes != null)
            button.HtmlAttributes = HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes);

        return Add(button);
    }

    public GriddlySettings ButtonSeparator(bool alignRight = false)
    {
        return Add(new GriddlyButton()
        {
            IsSeparator = true,
            AlignRight = alignRight
        });
    }

    public GriddlySettings ButtonHeader(string text, bool alignRight = false)
    {
        return Add(new GriddlyButton()
        {
            IsHeader = true,
            Text = text,
            AlignRight = alignRight
        });
    }

    public GriddlySettings SelectColumn(Expression<Func<object, object>> id, object summaryValue = null)
    {
        RowId(id, "id");

        return Add(new GriddlySelectColumn(this)
        {
            SummaryValue = summaryValue
        });
    }

    public GriddlySettings SelectColumn(Dictionary<string, Func<object, object>> ids, object summaryValue = null)
    {
        foreach (var x in ids)
        {
            RowIds[x.Key] = x.Value;
        }

        return Add(new GriddlySelectColumn(this)
        {
            SummaryValue = summaryValue
        });
    }

    SortField[] _defaultSort;

    public SortField[] DefaultSort
    {
        get
        {
            if (_defaultSort == null)
                _defaultSort = Columns
                    .Where(x => x.DefaultSort != null)
                    .OrderBy(x => x.DefaultSortOrder)
                    .ThenBy(x => Columns.IndexOf(x))
                    .Select(x => new SortField() { Field = x.ExpressionString, Direction = x.DefaultSort.Value }).ToArray();

            return _defaultSort;
        }
        set
        {
            _defaultSort = value;
        }
    }
}

public class GriddlySettings<TRow> : GriddlySettings
{
#if NETCOREAPP
    static MethodInfo _idPropGetter = typeof(TRow).GetProperty("Id", BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public)?.GetGetMethod();

    public GriddlySettings(IHtmlHelper html) : base(html) { }

    public GriddlySettings<TRow> Id(Expression<Func<TRow, object>> expression)
    {
        IdProperty = expression.Compile();
        return this;
    }
    public Func<TRow, object> IdProperty { get; set; }
    public override object TryGetId(object row)
    {
        if (IdProperty != null) 
        {
            return IdProperty((TRow)row);
        }
        else if(_idPropGetter != null)
        {
            try
            {
                return _idPropGetter.Invoke(row, null);
            }
            catch
            {
                return null;
            }
        }
        else
        {
            return null;
        }
    }
#else
    public GriddlySettings() { }

    /// <summary>
    /// This overload is only to provide a common signature between .NET Framework and .NET Core.
    /// </summary>
    /// <param name="html">Not used in .NET Framework.</param>
    public GriddlySettings(HtmlHelper html) : this() { }
#endif

#if NETFRAMEWORK
    public new Func<GriddlySettings<TRow>, object> FilterTemplate
#else
    public new Func<GriddlySettings<TRow>, IHtmlContent> FilterTemplate
#endif
    {
        set
        {
            if (value != null)
                base.FilterTemplate = (x) => value((GriddlySettings<TRow>)x);
            else
                base.FilterTemplate = null;
        }
    }

#if NETFRAMEWORK
    public new Func<GriddlySettings<TRow>, object> InlineFilterTemplate
#else
    public new Func<GriddlySettings<TRow>, IHtmlContent> InlineFilterTemplate
#endif
    {
        set
        {
            if (value != null)
                base.InlineFilterTemplate = (x) => value((GriddlySettings<TRow>)x);
            else
                base.InlineFilterTemplate = null;
        }
    }

    public new Func<TRow, object> RowClickUrl
    {
        set
        {
            if (value != null)
                base.RowClickUrl = (x) => value((TRow)x);
            else
                base.RowClickUrl = null;
        }
    }

    public new Func<TRow, object> RowClass
    {
        set
        {
            if (value != null)
                base.RowClass = (x) => value((TRow)x);
            else
                base.RowClass = null;
        }
    }

    public new Func<TRow, object> RowHtmlAttributes
    {
        set
        {
            if (value != null)
                base.RowHtmlAttributes = (x) => value((TRow)x);
            else
                base.RowHtmlAttributes = null;
        }
    }

    public GriddlySettings<TRow> RowId(Expression<Func<TRow, object>> expression, string name = null)
    {
        if (name == null)
        {
#if NETFRAMEWORK
            var meta = ModelMetadata.FromLambdaExpression(expression, new ViewDataDictionary<TRow>());
            name = ExpressionHelper.GetExpressionText(expression);
#else
            name = ExpressionHelper.GetExpressionText(expression, Html);
#endif
        }

        RowIds[name ?? "id"] = (x) => expression.Compile()((TRow)x);

        return this;
    }

    public GriddlySettings<TRow> Column<TProperty>(Expression<Func<TRow, TProperty>> expression, string caption = null, string format = null, string expressionString = null, SortDirection? defaultSort = null, string className = null, ColumnRenderMode renderMode = ColumnRenderMode.Both, string width = null, SummaryAggregateFunction? summaryFunction = null, object summaryValue = null, Func<TRow, object> template = null, Func<GriddlyColumn, object> headerTemplate = null, Func<GriddlyColumn, GriddlyFilter> filter = null, Func<TRow, object> htmlAttributes = null, object headerHtmlAttributes = null, int defaultSortOrder = 0, Expression<Func<TRow, object>> value = null, double? exportWidth = null, Func<TRow, string> linkUrl = null, bool visible = true, string columnId = null)
    {
        if (expression != null)
        {
#if NETFRAMEWORK
            var metadata = ModelMetadata.FromLambdaExpression<TRow, TProperty>(expression, new ViewDataDictionary<TRow>());
            string htmlFieldName = ExpressionHelper.GetExpressionText(expression);
#else
            string htmlFieldName = ExpressionHelper.GetExpressionText(expression, Html, out var metadata);
#endif

            Type type = metadata.ModelType;

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                type = Nullable.GetUnderlyingType(type);
            
            if (className == null)
            {
                if (type == typeof(byte) || type == typeof(sbyte) ||
                         type == typeof(short) || type == typeof(ushort) ||
                         type == typeof(int) || type == typeof(uint) ||
                         type == typeof(long) || type == typeof(ulong) ||
                         type == typeof(float) ||
                         type == typeof(double) ||
                         type == typeof(decimal))
                    className = Css.TextRight;
                else if (type == typeof(bool) ||
                         type == typeof(DateTime) || type.HasCastOperator<DateTime>())
                    className = Css.TextCenter;
            }

            if (caption == null)
                caption = metadata.DisplayName ?? metadata.PropertyName ?? htmlFieldName.Split('.').Last();

            if (expressionString == null)
                expressionString = htmlFieldName;

            if (template == null)
            {
                var compiledTemplate = expression.Compile();

                if (type == typeof(bool) && (BoolTrueHtml != null || BoolFalseHtml != null))
                    template = (row) => (compiledTemplate(row) as bool? == true) ? BoolTrueHtml : BoolFalseHtml;
                else
                    template = (row) => compiledTemplate(row);
            }
        }

        if (string.IsNullOrWhiteSpace(expressionString) && summaryFunction != null)
            throw new InvalidOperationException("Must specify an expression to use a summary function.");

        if (headerHtmlAttributes != null && !(headerHtmlAttributes is IDictionary<string, object>))
            headerHtmlAttributes = HtmlHelper.AnonymousObjectToHtmlAttributes(headerHtmlAttributes);

        var valueTemplate = value == null ? null : value.Compile();
        Add(new GriddlyColumn<TRow>(expression, caption, columnId)
        {
            Visible = visible,
            HeaderTemplate = headerTemplate,
            Template = template,
            Format = format,
            ExpressionString = expressionString,
            SummaryFunction = summaryFunction,
            SummaryValue = summaryValue,
            DefaultSort = defaultSort,
            DefaultSortOrder = defaultSortOrder,
            ClassName = className,
            RenderMode = renderMode,
            Width = width,
            ExportWidth = exportWidth,
            HtmlAttributesTemplate = htmlAttributes,
            HeaderHtmlAttributes = (IDictionary<string, object>)headerHtmlAttributes,
            UnderlyingValueTemplate = valueTemplate,
            LinkUrl = linkUrl
        }, filter);

        return this;
    }

    public GriddlySettings<TRow> Column(string caption = null, string format = null, string expressionString = null, SortDirection? defaultSort = null, string className = null, ColumnRenderMode renderMode = ColumnRenderMode.Both, string width = null, SummaryAggregateFunction? summaryFunction = null, object summaryValue = null, Func<TRow, object> template = null, Func<GriddlyColumn, object> headerTemplate = null, Func<GriddlyColumn, GriddlyFilter> filter = null, Func<TRow, object> htmlAttributes = null, object headerHtmlAttributes = null, int defaultSortOrder = 0, Expression<Func<TRow, object>> value = null, double? exportWidth = null, Func<TRow, string> linkUrl = null)
    {
        return Column<object>(null, caption, format, expressionString, defaultSort, className, renderMode, width, summaryFunction, summaryValue, template, headerTemplate, filter, htmlAttributes, headerHtmlAttributes, defaultSortOrder, value, exportWidth, linkUrl: linkUrl);
    }

    public GriddlySettings<TRow> SelectColumn(Expression<Func<TRow, object>> id, object summaryValue = null, Func<TRow, object> inputHtmlAttributesTemplate = null, string className = null)
    {
        RowId(id, "id");

        var col = new GriddlySelectColumn<TRow>(this)
        {
            SummaryValue = summaryValue,
            InputHtmlAttributesTemplate = inputHtmlAttributesTemplate
        };

        if (className != null)
            col.ClassName += " " + className;

        Add(col);

        return this;
    }

    public GriddlySettings<TRow> SelectColumn(Dictionary<string, Func<TRow, object>> ids, object summaryValue = null, string className = null)
    {
        foreach (var x in ids)
        {
            RowIds[x.Key] = (z) => x.Value((TRow)z);
        }

        var col = new GriddlySelectColumn(this)
        {
            SummaryValue = summaryValue
        };

        Add(col);

        if (className != null)
            col.ClassName += " " + className;

        return this;
    }

    public GriddlySettings<TRow> Button<TModel>(Func<TModel, object> argumentTemplate, string caption, string icon = null, GriddlyButtonAction action = GriddlyButtonAction.Navigate, string[] rowIds = null, object htmlAttributes = null)
    {
        var button = new GriddlyButton()
        {
            ArgumentTemplate = (x) => argumentTemplate((TModel)x),
            Text = caption,
            Icon = icon,
            Action = action,
            RowIds = rowIds
        };

        if (htmlAttributes != null)
            button.HtmlAttributes = HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes);

        Add(button);

        return this;
    }
}

[Flags]
public enum FilterMode
{
    None = 0,
    Inline = 1,
    Form = 2,
    Both = Inline | Form
}
