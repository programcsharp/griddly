using System;
using System.Linq;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Griddly.Mvc
{
    public class GriddlySelectColumn : GriddlyColumn
    {
        public Func<object, bool> IsRowSelectable { get; set; }

        public GriddlySelectColumn()
        {
            ClassName = "griddly-select align-center";
        }

        public virtual IDictionary<string, object> GenerateInputHtmlAttributes(object row)
        {
            return null;
        }
                
        public override HtmlString RenderCell(object row, GriddlySettings settings, bool encode = true)
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

                return new HtmlString(input.ToString(TagRenderMode.SelfClosing));
            }
            else
                return null;
        }

        public override object RenderCellValue(object row, bool stripHtml = false)
        {
            return null;
        }

        public override HtmlString RenderUnderlyingValue(object row)
        {
            return null;
        }
    }

    public class GriddlySelectColumn<TRow> : GriddlySelectColumn
    {
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
