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

        public static Func<GriddlyButton, object> IconTemplate = null;
        public static Func<GriddlyResultPage, object> DefaultFooterTemplate = null;
        public static Func<string, IEnumerable, ActionResult> HandleCustomReport = null;
        public static Action<GriddlySettings> BeforeRender = null;

        public GriddlySettings()
        {
            IdProperty = "Id";
            Columns = new List<GriddlyColumn>();
            Filters = new List<GriddlyFilter>();
            Buttons = new List<GriddlyButton>();
            FilterDefaults = new Dictionary<string, object>();
            ClassName = DefaultClassName;
            TableClassName = DefaultTableClassName;
            FooterTemplate = DefaultFooterTemplate;
            PageSize = DefaultPageSize;
            ShowFilterInitially = DefaultShowFilterInitially;
            HasInlineFilter = true;
        }

        public string IdProperty { get; set; }
        public string Title { get; set; }
        public string ClassName { get; set; }
        public string TableClassName { get; set; }
        public string OnClientRefresh { get; set; }
        public bool ShowFilterInitially { get; set; }

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
        public IDictionary<string, object> FilterDefaults { get; set; }

        public bool HasInlineFilter { get; set; }

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

        public GriddlySettings Button(Func<object, object> argumentTemplate, string caption, string icon = null, GriddlyButtonAction action = GriddlyButtonAction.Navigate, bool? enableOnSelection = null, string className = null, string target = null)
        {
            if (enableOnSelection == null)
                enableOnSelection = (action == GriddlyButtonAction.Ajax || action == GriddlyButtonAction.AjaxBulk || action == GriddlyButtonAction.Post);

            return Add(new GriddlyButton()
            {
                ArgumentTemplate = argumentTemplate,
                Text = caption,
                Icon = icon,
                Action = action,
                EnableOnSelection = enableOnSelection.Value,
                ClassName = className ?? GriddlySettings.DefaultButtonClassName,
                Target = target
            });
        }

        public GriddlySettings Button(string argument, string caption, string icon = null, GriddlyButtonAction action = GriddlyButtonAction.Navigate, bool? enableOnSelection = null, string className = null, string target = null)
        {
            if (enableOnSelection == null)
                enableOnSelection = (action == GriddlyButtonAction.Ajax || action == GriddlyButtonAction.AjaxBulk || action == GriddlyButtonAction.Post);

            return Add(new GriddlyButton()
            {
                Argument = argument,
                Text = caption,
                Icon = icon,
                Action = action,
                EnableOnSelection = enableOnSelection.Value,
                ClassName = className ?? GriddlySettings.DefaultButtonClassName,
                Target = target
            });
        }

        public GriddlySettings ButtonSeparator()
        {
            return Add(new GriddlyButton()
            {
                IsSeparator = true
            });
        }

        public GriddlySettings SelectColumn(Func<object, object> id)
        {
            return Add(new GriddlySelectColumn()
            {
                Id = id
            });
        }

        public SortField[] GetDefaultSort()
        {
            return Columns
                    .Where(x => x.DefaultSort != null)
                    .Select(x => new SortField() { Field = x.SortField, Direction = x.DefaultSort.Value }).ToArray();
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
        
        public GriddlySettings<TRow> Column<TProperty>(Expression<Func<TRow, TProperty>> template, string caption = null, string format = null, string sortField = null, SortDirection? defaultSort = null, string className = null, bool isExportOnly = false, string width = null, Func<GriddlyColumn, GriddlyFilter> filter = null)
        {
            var compiledTemplate = template.Compile();
            ModelMetadata metadata = ModelMetadata.FromLambdaExpression<TRow, TProperty>(template, new ViewDataDictionary<TRow>());
            string htmlFieldName = ExpressionHelper.GetExpressionText(template);

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

            if (sortField == null)
                sortField = htmlFieldName;

            if (type == typeof(bool) && (BoolTrueHtml != null || BoolFalseHtml != null))
            {
                return TemplateColumn(
                    (row) =>
                    {
                        return (compiledTemplate(row) as bool? == true) ? BoolTrueHtml : BoolFalseHtml;
                    },
                    caption, format, sortField, defaultSort, className, isExportOnly, width, filter
                );
            }

            Add(new GriddlyColumn<TRow>()
            {
                Template = (row) => compiledTemplate(row),
                Caption = caption,
                Format = format,
                SortField = sortField,
                DefaultSort = defaultSort,
                ClassName = className,
                IsExportOnly = isExportOnly,
                Width = width
            }, filter);

            return this;
        }

        public GriddlySettings<TRow> TemplateColumn(Func<TRow, object> template, string caption, string format = null, string sortField = null, SortDirection? defaultSort = null, string className = null, bool isExportOnly = false, string width = null, Func<GriddlyColumn, GriddlyFilter> filter = null)
        {
            Add(new GriddlyColumn<TRow>()
            {
                Template = template,
                Caption = caption,
                Format = format,
                SortField = sortField,
                DefaultSort = defaultSort,
                ClassName = className,
                IsExportOnly = isExportOnly,
                Width = width
            }, filter);

            return this;
        }

        public GriddlySettings<TRow> SelectColumn(Func<TRow, object> id)
        {
            Add(new GriddlySelectColumn()
            {
                Id = (x) => id((TRow)x)
            });

            return this;
        }

        public GriddlySettings<TRow> Button<TModel>(Func<TModel, object> argumentTemplate, string caption, string icon = null, GriddlyButtonAction action = GriddlyButtonAction.Navigate)
        {
            Add(new GriddlyButton()
            {
                ArgumentTemplate = (x) => argumentTemplate((TModel)x),
                Text = caption,
                Icon = icon,
                Action = action
            });

            return this;
        }
    }
}
