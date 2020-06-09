using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using System.Web;
#if NET45
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.WebPages;
#else
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
#endif

namespace Griddly.Mvc
{
    public abstract class GriddlyColumn
    {
        public GriddlyColumn()
        {
            HeaderHtmlAttributes = new RouteValueDictionary();
            RenderMode = ColumnRenderMode.Both;
        }

        public GriddlyColumn(LambdaExpression expression, string caption, string columnId) : this()
        {
            Expression = expression;
            Caption = caption;
            ColumnId = columnId != null ? columnId : expression != null ? GetIdFromExpression(expression) : Caption;
        }

        string GetIdFromExpression(LambdaExpression expression)
        {
            string expressionString = expression.Body.ToString();

            //trim the parameter for a cleaner expression
            string parameter = expression.Parameters.Single().Name;
            if (expressionString.StartsWith(parameter + "."))
                return expressionString.Substring(parameter.Length + 1);
            else
                return expressionString;
        }

        public LambdaExpression Expression { get; protected set; }
        public string Caption { get; set; }
        public string ExpressionString { get; set; }
        public string Format { get; set; }
        public SortDirection? DefaultSort { get; set; }
        public int DefaultSortOrder { get; set; }
        public string ClassName { get; set; }
        public string Width { get; set; }
        public double? ExportWidth { get; set; }
        public ColumnRenderMode RenderMode { get; set; }
        public bool Visible { get; set; } = true;
        public string ColumnId { get; set; }
        public SummaryAggregateFunction? SummaryFunction { get; set; }
        public object SummaryValue { get; set; }
        public IDictionary<string, object> HeaderHtmlAttributes { get; set; }

        public GriddlyFilter Filter { get; set; }

        public abstract HtmlString RenderUnderlyingValue(object row);
        public abstract HtmlString RenderCell(object row, GriddlySettings settings, bool encode = true);
        public abstract object RenderCellValue(object row, bool stripHtml = false);

        public virtual string RenderClassName(object row, GriddlyResultPage page)
        {
            return ClassName;
        }

        public virtual IDictionary<string, object> GenerateHtmlAttributes(object row, GriddlyResultPage page)
        {
            return null;
        }

        public virtual HtmlString RenderValue(object value, bool encode = true)
        {
            if (value == null)
                return null;

            if (Format == null)
            {
                if (value is DateTime || value is DateTime? || value.GetType().HasCastOperator<DateTime>())
                    Format = "d";
            }

            if (Format != null)
                value = string.Format("{0:" + Format + "}", value);
            else if (value is Enum)
                value = Extensions.ToStringDescription((Enum)value);
            else
                value = value.ToString();

            if (value == null)
                return null;

            if (value is HtmlString)
                return (HtmlString)value;
            else if (encode)
                return Extensions.GetHtmlView(value.ToString());
            else
                return new HtmlString(value.ToString());
        }
    }

    public class GriddlyColumn<TRow> : GriddlyColumn
    {
        public GriddlyColumn() : base()
        { }

        public GriddlyColumn(LambdaExpression expression, string caption, string columnId) : base(expression, caption, columnId) { }

        public Func<TRow, object> Template { get; set; }
        public Func<TRow, object> UnderlyingValueTemplate { get; set; }
        public Func<TRow, string> ClassNameTemplate { get; set; }
        public Func<TRow, object> HtmlAttributesTemplate { get; set; }
        public Func<TRow, string> LinkUrl { get; set; }

        static readonly Regex _htmlMatch = new Regex(@"<[^>]*>", RegexOptions.Compiled);

        public override string RenderClassName(object row, GriddlyResultPage page)
        {
            HashSet<string> classes = new HashSet<string>();

            if (!string.IsNullOrWhiteSpace(ClassName))
                classes.UnionWith(ClassName.Split(' '));

            SortField field = !string.IsNullOrWhiteSpace(ExpressionString) && page.SortFields != null ? page.SortFields.FirstOrDefault(x => x.Field == ExpressionString) : null;

            if (field != null)
                classes.Add("sorted_" + (field.Direction == SortDirection.Descending ? "d" : "a"));

            if (ClassNameTemplate != null)
            {
                string additional = ClassNameTemplate((TRow)row);

                if (!string.IsNullOrWhiteSpace(additional))
                    classes.UnionWith(additional.Split(' '));
            }

            if (classes.Count > 0)
                return string.Join(" ", classes);
            else
                return null;
        }

        public override IDictionary<string, object> GenerateHtmlAttributes(object row, GriddlyResultPage page)
        {
            if (HtmlAttributesTemplate == null)
                return null;

            RouteValueDictionary attributes = new RouteValueDictionary();

            object value = HtmlAttributesTemplate((TRow)row);

            if (value != null)
            {
                if (!(value is IDictionary<string, object>))
                    value = HtmlHelper.AnonymousObjectToHtmlAttributes(value);

                foreach (KeyValuePair<string, object> entry in (IDictionary<string, object>)value)
                    attributes.Add(entry.Key, entry.Value);
            }

            return attributes;
        }

        public override HtmlString RenderCell(object row, GriddlySettings settings, bool encode = true)
        {
            object value = null;

            try
            {
                value = Template((TRow)row);
            }
            catch (NullReferenceException)
            {
                // Eat
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error rendering column \"" + Caption + "\"", ex);
            }

            string valueString = null;

            if (value is HtmlString)
            {
                if (LinkUrl == null) return (HtmlString)value; //Return directly, to avoid converting to string and back to HtmlString unnecessarily
                else valueString = ((HtmlString)value).ToHtmlString();
            }
            else if (value is HelperResult && encode)
                valueString = ((HelperResult)value).ToHtmlString();
            else if (value is HelperResult && !encode)
                valueString = ((HelperResult)value).ToString();
            else
            {
                if (LinkUrl == null) return RenderValue(value, encode); //Return directly, to avoid converting to string and back to HtmlString unnecessarily
                else valueString = RenderValue(value, encode)?.ToHtmlString();
            }

            if (row != null && LinkUrl != null && !string.IsNullOrWhiteSpace(valueString))
            {
                string url = LinkUrl((TRow)row);
                if (!string.IsNullOrWhiteSpace(url))
                    valueString = string.Format(GriddlySettings.ColumnLinkTemplate, url, valueString);
            }

            return new HtmlString(valueString);
        }

        public override HtmlString RenderUnderlyingValue(object row)
        {
            if (UnderlyingValueTemplate == null) return null;

            object value = null;

            try
            {
                value = UnderlyingValueTemplate((TRow)row);
            }
            catch (NullReferenceException)
            {
                // Eat
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error rendering underlying value or column \"" + Caption + "\"", ex);
            }

            if (value == null)
                return null;
            else if (value is HtmlString)
                return (HtmlString)value;
            else
                return new HtmlString(value.ToString());
        }

        public override object RenderCellValue(object row, bool stripHtml = false)
        {
            object value = null;

            try
            {
                value = Template((TRow)row);
            }
            catch (NullReferenceException)
            {
                // Eat
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error rendering column \"" + Caption + "\"", ex);
            }


            // TODO: test if we need to match separately -- maybe we get a real string here and could strip?
            if (value is HelperResult)
                value = new HtmlString(((HelperResult)value).ToString());

            if (value is HtmlString)
                value = value.ToString();
            else if (value is Enum)
                value = Extensions.ToStringDescription((Enum)value);
            else if (value != null && value.GetType().HasCastOperator<DateTime>())
                // value = (DateTime)value; -- BAD: can't unbox a value type as a different type
                value = Convert.ChangeType(value, typeof(DateTime));

            if (stripHtml && value is string)
                value = HttpUtility.HtmlDecode(_htmlMatch.Replace(value.ToString(), "").Trim().Replace("  ", " "));

            return value;
        }
    }

    public enum SummaryAggregateFunction
    {
        Sum = 1,
        Average = 2,
        Min = 3,
        Max = 4
    }

    public enum ColumnRenderMode
    {
        View = 1 << 0,
        Export = 1 << 1,
        Both = View | Export
    }
}
