using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Helpers;
using System.Web.WebPages;

namespace Griddly.Mvc
{
    public abstract class GriddlyColumn
    {
        public string Caption { get; set; }
        public string ExpressionString { get; set; }
        public string Format { get; set; }
        public SortDirection? DefaultSort { get; set; }
        public string ClassName { get; set; }
        public string Width { get; set; }
        public bool IsExportOnly { get; set; }
        public SummaryAggregateFunction? SummaryFunction { get; set; }
        public object SummaryValue { get; set; }

        public GriddlyFilter Filter { get; set; }

        public abstract HtmlString RenderCell(object row, GriddlySettings settings, bool encode = true);
        public abstract object RenderCellValue(object row, bool stripHtml = false);
        public abstract string RenderClassName(object row, GriddlyResultPage page);

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
        public Func<TRow, object> Template { get; set; }
        public Func<TRow, string> ClassNameTemplate { get; set; }

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

            if (value is HtmlString)
                return (HtmlString)value;
            else if (value is HelperResult && encode)
                return new HtmlString(((HelperResult)value).ToHtmlString());
            else if (value is HelperResult && !encode)
                return new HtmlString(((HelperResult)value).ToString());
            else
                return RenderValue(value, encode);
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
}
