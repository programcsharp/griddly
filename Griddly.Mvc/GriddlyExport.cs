using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.Routing;

namespace Griddly.Mvc
{
    public class GriddlyExport
    {
        public GriddlyExport(string name, bool useGridColumns = false)
        {
            this.UseGridColumns = useGridColumns;
            this.Name = name;
            this.Columns = new List<GriddlyColumn>();
        }
        public string Name { get; set; }
        public bool UseGridColumns { get; set; }
        public List<GriddlyColumn> Columns { get; set; }
    }
    public class GriddlyExport<TRow> : GriddlyExport
    {
        public GriddlyExport(string name, bool useGridColumns = false)
            : base(name, useGridColumns)
        {
        }
        public GriddlyExport<TRow> Column<TProperty>(Expression<Func<TRow, TProperty>> expression, string caption = null, string format = null, string expressionString = null, SortDirection? defaultSort = null, string className = null, ColumnRenderMode renderMode = ColumnRenderMode.Both, string width = null, SummaryAggregateFunction? summaryFunction = null, object summaryValue = null, Func<TRow, object> template = null, Func<TRow, object> htmlAttributes = null, object headerHtmlAttributes = null, int defaultSortOrder = 0, Expression<Func<TRow, object>> value = null)
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

                    //if (type == typeof(bool) && (BoolTrueHtml != null || BoolFalseHtml != null))
                    //    template = (row) => (compiledTemplate(row) as bool? == true) ? BoolTrueHtml : BoolFalseHtml;
                    //else
                    template = (row) => compiledTemplate(row);
                }
            }

            if (string.IsNullOrWhiteSpace(expressionString) && summaryFunction != null)
                throw new InvalidOperationException("Must specify an expression to use a summary function.");

            if (headerHtmlAttributes != null && !(headerHtmlAttributes is IDictionary<string, object>))
                headerHtmlAttributes = HtmlHelper.AnonymousObjectToHtmlAttributes(headerHtmlAttributes);

            var valueTemplate = value == null ? null : value.Compile();
            Columns.Add(new GriddlyColumn<TRow>()
            {
                Template = template,
                Caption = caption,
                Format = format,
                ExpressionString = expressionString,
                SummaryFunction = summaryFunction,
                SummaryValue = summaryValue,
                DefaultSort = defaultSort,
                DefaultSortOrder = defaultSortOrder,
                ClassName = className,
                RenderMode = renderMode,
                Width = width,
                HtmlAttributesTemplate = htmlAttributes,
                HeaderHtmlAttributes = (IDictionary<string, object>)headerHtmlAttributes,
                UnderlyingValueTemplate = valueTemplate
            });

            return this;
        }

        public GriddlyExport<TRow> Column(string caption = null, string format = null, string expressionString = null, SortDirection? defaultSort = null, string className = null, ColumnRenderMode renderMode = ColumnRenderMode.Both, string width = null, SummaryAggregateFunction? summaryFunction = null, object summaryValue = null, Func<TRow, object> template = null, Func<TRow, object> htmlAttributes = null, object headerHtmlAttributes = null, int defaultSortOrder = 0, Expression<Func<TRow, object>> value = null)
        {
            return Column<object>(null, caption, format, expressionString, defaultSort, className, renderMode, width, summaryFunction, summaryValue, template, htmlAttributes, headerHtmlAttributes, defaultSortOrder, value);
        }
    }
}
