using System;
using System.ComponentModel;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Helpers;
using System.Web.WebPages;

namespace Griddly.Mvc
{
    public abstract class GriddlyColumn
    {
        public string Caption { get; set; }
        public string SortField { get; set; }
        public string Format { get; set; }
        public SortDirection? DefaultSort { get; set; }
        public string ClassName { get; set; }
        public string Width { get; set; }
        public bool IsExportOnly { get; set; }

        public abstract HtmlString RenderCell(object row, bool encode = true);
        public abstract object RenderCellValue(object row);

        protected virtual HtmlString RenderValue(object value, bool encode = true)
        {
            if (value == null)
                return null;

            if (Format == null)
            {
                if (value is DateTime || value is DateTime? || value.GetType().Name.Contains("M3DateTime"))
                    Format = "d";
            }

            if (Format != null)
                value = string.Format("{0:" + Format + "}", value);
            else if (value is Enum)
                value = ToStringDescription((Enum)value);
            else
                value = value.ToString();

            if (value is HtmlString)
                return (HtmlString)value;
            else if (encode)
                return GetHtmlView(value.ToString());
            else
                return new HtmlString(value.ToString());
        }

        static readonly Regex _urlRegex = new Regex(@"(http|ftp|https):\/\/[\w\-_]+(\.[\w\-_]+)+([\w\-\.,@?^=%&amp;:/~\+#]*[\w\-\@?^=%&amp;/~\+#])?", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        static HtmlString GetHtmlView(string value, bool turnUrlsIntoLinks = false)
        {
            if (value == null)
                return null;

            value = HttpUtility.HtmlEncode(value).Replace("  ", "&nbsp; ")
                    .Replace("\r\n", "<br/>").Replace("\r", "<br/>").Replace("\n", "<br/>");

            if (turnUrlsIntoLinks)
            {
                foreach (Match match in _urlRegex.Matches(value))
                {
                    if (match != null && !string.IsNullOrWhiteSpace(match.Value))
                        value = value.Replace(match.Value, string.Format("<a href=\"{0}\">{1}</a>", HttpUtility.HtmlAttributeEncode(match.Value), match.Value));
                }
            }

            return new HtmlString(value);
        }

        protected static string ToStringDescription(Enum value)
        {
            if (value == null)
                return null;

            FieldInfo fi = value.GetType().GetField(value.ToString());

            DescriptionAttribute[] attributes = null;

            if (fi != null)
                attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);

            if (attributes != null && attributes.Length > 0)
                return attributes[0].Description;
            else
                return value.ToString();
        }
    }

    public class GriddlyColumn<TRow> : GriddlyColumn
    {
        public Func<TRow, object> Template { get; set; }

        public override HtmlString RenderCell(object row, bool encode = true)
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

        public override object RenderCellValue(object row)
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
                value = value.ToString();
            else if (value is HelperResult)
                value = new HtmlString(((HelperResult)value).ToString());
            else if (value is Enum)
                value = ToStringDescription((Enum)value);
            else if (value != null && value.GetType().Name.Contains("M3DateTime"))
                value = (DateTime?)value;

            return value;
        }
    }
}
