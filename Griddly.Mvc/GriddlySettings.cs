using System;
using System.Linq;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Web.Mvc;
using System.Web;

namespace Griddly.Mvc
{
    public class GriddlySettings
    {
        public static string DefaultClassName = null;
        public static string ButtonTemplate = "~/Views/Shared/Griddly/BootstrapButton.cshtml";
        public static string ButtonListTemplate = "~/Views/Shared/Griddly/ButtonStrip.cshtml";
        public static string BoolTrueHtml = "<span class=\"icon20 check_gray\"></span>";
        public static string BoolFalseHtml = null;
        public static int? DefaultPageSize = null;

        public static Func<GriddlyButton, object> IconTemplate = null;
        public static Func<GriddlyResultPage, object> DefaultFooterTemplate = null;

        public GriddlySettings()
        {
            Columns = new List<GriddlyColumn>();
            Buttons = new List<GriddlyButton>();
            FilterDefaults = new Dictionary<string, object>();
            ClassName = DefaultClassName;
            FooterTemplate = DefaultFooterTemplate;
            PageSize = DefaultPageSize;
        }

        public string Title { get; set; }
        public string ClassName { get; set; }
        public string OnClientRefresh { get; set; }

        public int? PageSize { get; set; }

        public List<GriddlyColumn> Columns { get; set; }

        public List<GriddlyButton> Buttons { get; set; }

        public Func<object, object> BeforeTemplate { get; set; }
        public Func<object, object> AfterTemplate { get; set; }
        public Func<object, object> FilterTemplate { get; set; }
        public bool FilterInline { get; set; }
        public Func<object, object> RowClickUrl { get; set; }
        public string RowClickModal { get; set; }
        public Func<object, object> RowClass { get; set; }

        public Func<GriddlyResultPage, object> FooterTemplate { get; set; }
        public IDictionary<string, object> FilterDefaults { get; set; }

        public virtual bool HasFilterTemplate
        {
            get { return FilterTemplate != null; }
        }

        public virtual bool HasRowClickUrl
        {
            get { return RowClickUrl != null; }
        }

        public virtual bool HasRowClass
        {
            get { return RowClass != null; }
        }

        public virtual object RenderFilterTemplate(object o)
        {
            return FilterTemplate(o);
        }

        public virtual object RenderRowClickUrl(object o)
        {
            return RowClickUrl(o);
        }

        public virtual object RenderRowClass(object o)
        {
            return RowClass(o);
        }

        public GriddlySettings Add(GriddlyColumn column)
        {
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
            return Add(new GriddlyButton()
            {
                ArgumentTemplate = argumentTemplate,
                Text = caption,
                Icon = icon,
                Action = action,
                EnableOnSelection = enableOnSelection,
                ClassName = className,
                Target = target
            });
        }

        public GriddlySettings Button(string argument, string caption, string icon = null, GriddlyButtonAction action = GriddlyButtonAction.Navigate, bool? enableOnSelection = null, string className = null, string target = null)
        {
            return Add(new GriddlyButton()
            {
                Argument = argument,
                Text = caption,
                Icon = icon,
                Action = action,
                EnableOnSelection = enableOnSelection,
                ClassName = className,
                Target = target
            });
        }

        public GriddlySettings SelectColumn(Func<object, object> id)
        {
            return Add(new GriddlySelectColumn()
            {
                Id = id
            });
        }
    }

    public class GriddlySettings<TRow> : GriddlySettings
    {
        public new Func<TRow, object> FilterTemplate { get; set; }
        public new Func<TRow, object> RowClickUrl { get; set; }
        public new Func<TRow, object> RowClass { get; set; }

        public override bool HasFilterTemplate
        {
            get { return FilterTemplate != null; }
        }

        public override bool HasRowClickUrl
        {
            get { return RowClickUrl != null; }
        }

        public override bool HasRowClass
        {
            get { return RowClass != null; }
        }

        public override object RenderFilterTemplate(object o)
        {
            return FilterTemplate((TRow)o);
        }

        public override object RenderRowClickUrl(object o)
        {
            return RowClickUrl((TRow)o);
        }

        public override object RenderRowClass(object o)
        {
            return RowClass((TRow)o);
        }

        public GriddlySettings<TRow> Column<TProperty>(Expression<Func<TRow, TProperty>> template, string caption, string format = null, string sortField = null, string defaultSort = null, string className = null, bool isExportOnly = false)
        {
            ModelMetadata metadata = ModelMetadata.FromLambdaExpression<TRow, TProperty>(template, new ViewDataDictionary<TRow>());

            if (className == null)
            {
                if (metadata.ModelType == typeof(bool) || metadata.ModelType == typeof(bool?) ||
                    metadata.ModelType == typeof(DateTime) || metadata.ModelType == typeof(DateTime?) || metadata.ModelType.Name.Contains("M3DateTime"))
                    className = "align-center";
                else if (metadata.ModelType == typeof(byte) || metadata.ModelType == typeof(sbyte) ||
                         metadata.ModelType == typeof(short) || metadata.ModelType == typeof(ushort) ||
                         metadata.ModelType == typeof(int) || metadata.ModelType == typeof(uint) ||
                         metadata.ModelType == typeof(long) || metadata.ModelType == typeof(ulong) ||
                         metadata.ModelType == typeof(float) || 
                         metadata.ModelType == typeof(double) || 
                         metadata.ModelType == typeof(decimal)) 
                    className = "align-right";
            }

            if (metadata.ModelType == typeof(bool) || metadata.ModelType == typeof(bool?))
            {
                return TemplateColumn(
                    (row) =>
                    {
                        string value = (template.Compile()(row) as bool? == true) ? BoolTrueHtml : BoolFalseHtml;

                        if (!string.IsNullOrWhiteSpace(value))
                            return new HtmlString(value);
                        else
                            return null;
                    },
                    caption, format, sortField, defaultSort, className, isExportOnly
                    );
            }

            Add(new GriddlyColumn<TRow>()
            {
                Template = (row) => template.Compile()(row),
                Caption = caption,
                Format = format,
                SortField = sortField ?? metadata.PropertyName,
                DefaultSort = defaultSort,
                ClassName = className,
                IsExportOnly = isExportOnly
            });

            return this;
        }

        public GriddlySettings<TRow> TemplateColumn(Func<TRow, object> template, string caption, string format = null, string sortField = null, string defaultSort = null, string className = null, bool isExportOnly = false)
        {
            Add(new GriddlyColumn<TRow>()
            {
                Template = template,
                Caption = caption,
                Format = format,
                SortField = sortField,
                DefaultSort = defaultSort,
                ClassName = className,
                IsExportOnly = isExportOnly
            });

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
