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

            Ids = new Dictionary<string, Func<object, object>>();
        }

        public Dictionary<string, Func<object, object>> Ids { get; set; }

        public override HtmlString RenderCell(object row, bool encode = true)
        {
            TagBuilder input = new TagBuilder("input");

            input.Attributes["name"] = "_rowselect";
            input.Attributes["type"] = "checkbox";

            foreach (var x in this.Ids)
            {
                input.Attributes[x.Key] = x.Value(row).ToString();
            }

            return new HtmlString(input.ToString(TagRenderMode.SelfClosing));
        }

        public override object RenderCellValue(object row, bool stripHtml = false)
        {
            return null;
        }

        public override string RenderClassName(object row, GriddlyResultPage page)
        {
            return ClassName;
        }
    }
}
