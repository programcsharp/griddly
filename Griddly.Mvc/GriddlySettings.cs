using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;

namespace Griddly.Mvc
{
    public abstract class GriddlySettings
    {
        public static string DefaultClassName = null;
        public static string DefaultTableClassName = "table table-bordered table-hover";
        public static string DefaultButtonClassName = null;
        public static string ButtonTemplate = "~/Views/Shared/Griddly/BootstrapButton.cshtml";
        public static string ButtonListTemplate = "~/Views/Shared/Griddly/ButtonStrip.cshtml";
        public static HtmlString BoolTrueHtml = null;
        public static HtmlString BoolFalseHtml = null;
        public static int? DefaultPageSize = null;
        public static bool DefaultShowFilterInitially = true;
        public static bool DefaultShowRowSelectCount = true;

        public static Func<GriddlyButton, object> IconTemplate = null;
        public static Func<GriddlyResultPage, object> DefaultFooterTemplate = null;
        public static Func<string, IEnumerable, ActionResult> HandleCustomReport = null;
        public static Action<GriddlySettings> BeforeRender = null;
        public static Action<GriddlySettings> OnGriddlyResultExecuting = null;

        public GriddlySettings()
        {
            IdProperty = "Id";
            HasInlineFilter = true;

            Columns = new List<GriddlyColumn>();
            Filters = new List<GriddlyFilter>();
            Buttons = new List<GriddlyButton>();
            RowIds = new Dictionary<string, Func<object, object>>();

            ClassName = DefaultClassName;
            TableClassName = DefaultTableClassName;
            FooterTemplate = DefaultFooterTemplate;
            PageSize = DefaultPageSize;
            ShowFilterInitially = DefaultShowFilterInitially;
            ShowRowSelectCount = DefaultShowRowSelectCount;
        }

        public string[] DefaultRowIds { get; set; }
        public string IdProperty { get; set; }
        public string Title { get; set; }
        public string ClassName { get; set; }
        public string TableClassName { get; set; }
        public string OnClientRefresh { get; set; }
        public bool ShowFilterInitially { get; set; }
        public bool ShowRowSelectCount { get; set; }

        public int? PageSize { get; set; }
        public int? MaxPageSize { get; set; }

        public List<GriddlyColumn> Columns { get; set; }
        public List<GriddlyFilter> Filters { get; set; }
        public List<GriddlyButton> Buttons { get; set; }

        public Func<object, object> BeforeTemplate { get; set; }
        public Func<object, object> AfterTemplate { get; set; }
        public Func<GriddlySettings, object> FilterTemplate { get; set; }
        public Func<GriddlySettings, object> InlineFilterTemplate { get; set; }

        public Func<object, object> RowClickUrl { get; set; }
        public string RowClickModal { get; set; }
        public Func<object, object> RowClass { get; set; }

        public Func<GriddlyResultPage, object> FooterTemplate { get; set; }

        public bool HasInlineFilter { get; set; }

        public Dictionary<string, Func<object, object>> RowIds { get; protected set; }

        public virtual bool HasRowClickUrl
        {
            get { return RowClickUrl != null; }
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

        public GriddlySettings RowId(Expression<Func<object, object>> expression, string name = null)
        {
            if (name == null)
            {
                var meta = ModelMetadata.FromLambdaExpression(expression, new ViewDataDictionary<object>());
                name = ExpressionHelper.GetExpressionText(expression);
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
                    if (HasInlineFilter)
                        column.Filter = filterDef;
                    else
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

        /*public GriddlySettings Button(string url, string caption, string icon = null)
        {
            return Add(new GriddlyButton()
                {
                    HRef = url,
                    Text = caption,
                    Icon = icon
                });
        }*/

        public GriddlySettings Button(Func<object, object> argumentTemplate, string caption, string icon = null, GriddlyButtonAction action = GriddlyButtonAction.Navigate, bool? enableOnSelection = null, string className = null, string target = null, string[] rowIds = null, object htmlAttributes = null)
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
                RowIds = rowIds
            };

            if (htmlAttributes != null)
                button.HtmlAttributes = HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes);

            return Add(button);
        }

        public GriddlySettings Button(string argument, string caption, string icon = null, GriddlyButtonAction action = GriddlyButtonAction.Navigate, bool? enableOnSelection = null, string className = null, string target = null, string[] rowIds = null, object htmlAttributes = null)
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
                RowIds = rowIds
            };

            if (htmlAttributes != null)
                button.HtmlAttributes = HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes);

            return Add(button);
        }

        public GriddlySettings ButtonSeparator()
        {
            return Add(new GriddlyButton()
            {
                IsSeparator = true
            });
        }

        public GriddlySettings SelectColumn(Expression<Func<object, object>> id, object summaryValue = null)
        {
            RowId(id, "id");

            return Add(new GriddlySelectColumn()
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

            return Add(new GriddlySelectColumn()
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
        public new Func<GriddlySettings<TRow>, object> FilterTemplate
        {
            set
            {
                if (value != null)
                    base.FilterTemplate = (x) => value((GriddlySettings<TRow>)x);
                else
                    base.FilterTemplate = null;
            }
        }

        public new Func<GriddlySettings<TRow>, object> InlineFilterTemplate
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

        public GriddlySettings<TRow> RowId(Expression<Func<TRow, object>> expression, string name = null)
        {
            if (name == null)
            {
                var meta = ModelMetadata.FromLambdaExpression(expression, new ViewDataDictionary<TRow>());
                name = ExpressionHelper.GetExpressionText(expression);
            }

            RowIds[name ?? "id"] = (x) => expression.Compile()((TRow)x);

            return this;
        }
        
        public GriddlySettings<TRow> Column<TProperty>(Expression<Func<TRow, TProperty>> expression, string caption = null, string format = null, string expressionString = null, SortDirection? defaultSort = null, string className = null, bool isExportOnly = false, string width = null, SummaryAggregateFunction? summaryFunction = null, object summaryValue = null, Func<TRow, object> template = null, Func<GriddlyColumn, GriddlyFilter> filter = null)
        {
            ModelMetadata metadata = null;

            if (expression != null)
            {
                metadata = ModelMetadata.FromLambdaExpression<TRow, TProperty>(expression, new ViewDataDictionary<TRow>());
                string htmlFieldName = ExpressionHelper.GetExpressionText(expression);

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
                        className = "align-right";
                    else if (type == typeof(bool) ||
                             type == typeof(DateTime) || type.HasCastOperator<DateTime>())
                        className = "align-center";
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

            Add(new GriddlyColumn<TRow>()
            {
                Template = template,
                Caption = caption,
                Format = format,
                ExpressionString = expressionString,
                SummaryFunction = summaryFunction,
                SummaryValue = summaryValue,
                DefaultSort = defaultSort,
                ClassName = className,
                IsExportOnly = isExportOnly,
                Width = width
            }, filter);

            return this;
        }

        public GriddlySettings<TRow> Column(string caption = null, string format = null, string expressionString = null, SortDirection? defaultSort = null, string className = null, bool isExportOnly = false, string width = null, SummaryAggregateFunction? summaryFunction = null, object summaryValue = null, Func<TRow, object> template = null, Func<GriddlyColumn, GriddlyFilter> filter = null)
        {
            return Column<object>(null, caption, format, expressionString, defaultSort, className, isExportOnly, width, summaryFunction, summaryValue, template, filter);
        }
        //public GriddlySettings<TRow> TemplateColumn(Func<TRow, object> template, string caption, string format = null, string sortField = null, SortDirection? defaultSort = null, string className = null, bool isExportOnly = false, string width = null, Func<GriddlyColumn, GriddlyFilter> filter = null)
        //{
        //    Add(new GriddlyColumn<TRow>()
        //    {
        //        Template = template,
        //        Caption = caption,
        //        Format = format,
        //        SortField = sortField,
        //        DefaultSort = defaultSort,
        //        ClassName = className,
        //        IsExportOnly = isExportOnly,
        //        Width = width
        //    }, filter);

        //    return this;
        //}

        public GriddlySettings<TRow> SelectColumn(Expression<Func<TRow, object>> id, object summaryValue = null)
        {
            RowId(id, "id");

            Add(new GriddlySelectColumn()
            {
                SummaryValue = summaryValue
            });

            return this;
        }

        public GriddlySettings<TRow> SelectColumn(Dictionary<string, Func<TRow, object>> ids, object summaryValue = null)
        {
            foreach (var x in ids)
            {
                RowIds[x.Key] = (z) => x.Value((TRow)z);
            }

            Add(new GriddlySelectColumn()
            {
                SummaryValue = summaryValue
            });

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
}
