using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
#if NET45_OR_GREATER
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.Routing;
#else
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.Rendering;
#endif

namespace Griddly.Mvc
{
    public class GriddlyExport
    {
#if NET45_OR_GREATER
        public GriddlyExport(string name, bool useGridColumns = false)
#else
        public GriddlyExport(IHtmlHelper html, string name, bool useGridColumns = false)
#endif

        {
            this.UseGridColumns = useGridColumns;
            this.Name = name;
            this.Columns = new List<GriddlyColumn>();
#if !NET45_OR_GREATER
            this.Html = html;
#endif
        }
        public string Name { get; set; }
        public bool UseGridColumns { get; set; }
        public List<GriddlyColumn> Columns { get; set; }
#if !NET45_OR_GREATER
        public IHtmlHelper Html { get; set; }
#endif
    }
    public class GriddlyExport<TRow> : GriddlyExport
    {
#if NET45_OR_GREATER
        public GriddlyExport(string name, bool useGridColumns = false)
            : base(name, useGridColumns)
#else
        public GriddlyExport(IHtmlHelper html, string name, bool useGridColumns = false)
            : base(html, name, useGridColumns)
#endif
        {
        }
        public GriddlyExport<TRow> Column<TProperty>(Expression<Func<TRow, TProperty>> expression, string caption = null, string format = null, string expressionString = null, SortDirection? defaultSort = null, string className = null, ColumnRenderMode renderMode = ColumnRenderMode.Both, string width = null, SummaryAggregateFunction? summaryFunction = null, object summaryValue = null, Func<TRow, object> template = null, Func<TRow, object> htmlAttributes = null, object headerHtmlAttributes = null, int defaultSortOrder = 0, Expression<Func<TRow, object>> value = null, double? exportWidth = null, bool visible = true, string columnId = null)
        {
            if (expression != null)
            {
#if NET45_OR_GREATER
                ModelMetadata metadata = ModelMetadata.FromLambdaExpression<TRow, TProperty>(expression, new ViewDataDictionary<TRow>());
                string htmlFieldName = ExpressionHelper.GetExpressionText(expression);
#else
                string htmlFieldName = ExpressionHelper.GetExpressionText(expression, Html, out var metadata);
#endif
                Type type = metadata.ModelType;

                if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                    type = Nullable.GetUnderlyingType(type);

                if (caption == null)
                    caption = metadata.DisplayName ?? metadata.PropertyName ?? htmlFieldName.Split('.').Last();

                if (expressionString == null)
                    expressionString = htmlFieldName;

                if (template == null)
                {
                    var compiledTemplate = expression.Compile();

                    template = (row) => compiledTemplate(row);
                }
            }

            if (string.IsNullOrWhiteSpace(expressionString) && summaryFunction != null)
                throw new InvalidOperationException("Must specify an expression to use a summary function.");

            if (headerHtmlAttributes != null && !(headerHtmlAttributes is IDictionary<string, object>))
                headerHtmlAttributes = HtmlHelper.AnonymousObjectToHtmlAttributes(headerHtmlAttributes);

            var valueTemplate = value == null ? null : value.Compile();
            Columns.Add(new GriddlyColumn<TRow>(expression, caption, columnId)
            {
                Visible = visible,
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
                UnderlyingValueTemplate = valueTemplate
            });

            return this;
        }

        public GriddlyExport<TRow> Column(string caption = null, string format = null, string expressionString = null, SortDirection? defaultSort = null, string className = null, ColumnRenderMode renderMode = ColumnRenderMode.Both, string width = null, SummaryAggregateFunction? summaryFunction = null, object summaryValue = null, Func<TRow, object> template = null, Func<TRow, object> htmlAttributes = null, object headerHtmlAttributes = null, int defaultSortOrder = 0, Expression<Func<TRow, object>> value = null, double? exportWidth = null)
        {
            return Column<object>(null, caption, format, expressionString, defaultSort, className, renderMode, width, summaryFunction, summaryValue, template, htmlAttributes, headerHtmlAttributes, defaultSortOrder, value, exportWidth);
        }
    }
}
