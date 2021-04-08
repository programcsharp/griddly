using System;
using System.Linq;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Web;
using System.IO;
using System.Text;
#if NET45
using System.Web.Mvc;
using System.Web.Routing;
#else
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using System.Text.Encodings.Web;
#endif

namespace Griddly.Mvc
{
    public class GriddlySelectColumn : GriddlyColumn
    {
        public GriddlySelectColumn() : base() { }

        public Func<object, bool> IsRowSelectable { get; set; }

        public GriddlySelectColumn(GriddlySettings settings)
        {
            ClassName = $"griddly-select {settings.Css.TextCenter}";
        }

        public virtual IDictionary<string, object> GenerateInputHtmlAttributes(object row)
        {
            return null;
        }
                
        public override HtmlString RenderCell(object row, GriddlySettings settings,
#if NET45
            HtmlHelper html,
#else
            IHtmlHelper html,
#endif
            bool encode = true)
        {
            if (IsRowSelectable?.Invoke(row) != false)
            {
                TagBuilder input = new TagBuilder("input");

                input.Attributes["name"] = "_rowselect";
                input.Attributes["type"] = "checkbox";

                if (settings.RowIds.Any())
                {
                    bool valueSet = false;
                    string key = "";
                    foreach (var x in settings.RowIds)
                    {
                        string val = "";
                        object result = x.Value(row);
                        if (result != null)
                            val = result.ToString();

                        input.Attributes["data-" + x.Key] = val;
                        key += "_" + val;

                        if (!valueSet)
                        {
                            input.Attributes["value"] = val;
                            valueSet = true;
                        }
                    }

                    input.Attributes["data-rowkey"] = key;
                }

                var inputAttributes = GenerateInputHtmlAttributes(row);

                if (inputAttributes != null)
                {
                    input.MergeAttributes(inputAttributes);
                }

#if NET45
                return new HtmlString(input.ToString(TagRenderMode.SelfClosing));
#else
                var sb = new StringBuilder();
                using (TextWriter tw = new StringWriter(sb))
                {
                    input.RenderSelfClosingTag().WriteTo(tw, HtmlEncoder.Default);
                    return new HtmlString(sb.ToString());
                }
#endif
            }
            else
                return null;
        }

        public override object RenderCellValue(object row,
#if NET45
            HttpContextBase httpContext,
#else
            HttpContext httpContext,
#endif
            bool stripHtml = false)
        {
            return null;
        }

        public override HtmlString RenderUnderlyingValue(object row,
#if NET45
            HtmlHelper html
#else
            IHtmlHelper html
#endif
        )
        {
            return null;
        }
    }

    public class GriddlySelectColumn<TRow> : GriddlySelectColumn
    {
        public GriddlySelectColumn(GriddlySettings<TRow> settings) : base(settings)
        {
        }

        public Func<TRow, object> InputHtmlAttributesTemplate { get; set; }

        public new Func<TRow, bool> IsRowSelectable
        {
            set
            {
                if (value != null)
                    base.IsRowSelectable = (x) => value((TRow)x);
                else
                    base.IsRowSelectable = null;
            }
        }

        public override IDictionary<string, object> GenerateInputHtmlAttributes(object row)
        {
            if (InputHtmlAttributesTemplate == null)
                return null;

            RouteValueDictionary attributes = new RouteValueDictionary();

            object value = InputHtmlAttributesTemplate((TRow)row);

            if (value != null)
            {
                if (!(value is IDictionary<string, object>))
                    value = HtmlHelper.AnonymousObjectToHtmlAttributes(value);

                foreach (KeyValuePair<string, object> entry in (IDictionary<string, object>)value)
                    attributes.Add(entry.Key, entry.Value);
            }

            return attributes;
        }
    }
}
