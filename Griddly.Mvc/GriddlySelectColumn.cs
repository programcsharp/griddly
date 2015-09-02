using System;
using System.Linq;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;

namespace Griddly.Mvc
{
    public class GriddlySelectColumn : GriddlyColumn
    {
        public GriddlySelectColumn()
        {
            ClassName = "griddly-select align-center";
        }
        
        public override HtmlString RenderCell(object row, GriddlySettings settings, bool encode = true)
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

            return new HtmlString(input.ToString(TagRenderMode.SelfClosing));
        }

        public override object RenderCellValue(object row, bool stripHtml = false)
        {
            return null;
        }
    }
}
