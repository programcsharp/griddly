using System;
using System.Web;
using System.Web.Mvc;

namespace Griddly.Mvc
{
    public class GriddlySelectColumn : GriddlyColumn
    {
        public GriddlySelectColumn()
        {
            ClassName = "griddly-select";
        }

        public Func<object, object> Id { get; set; }

        public override HtmlString RenderCell(object row, bool encode = true)
        {
            TagBuilder input = new TagBuilder("input");

            input.Attributes["name"] = "_rowselect";
            input.Attributes["value"] = Id(row).ToString();
            input.Attributes["type"] = "checkbox";

            return new HtmlString(input.ToString(TagRenderMode.SelfClosing));
        }

        public override object RenderCellValue(object row)
        {
            return null;
        }
    }
}
