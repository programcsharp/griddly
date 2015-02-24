using System;

namespace Griddly.Mvc
{
    public class GriddlyHtmlFilter : GriddlyFilter
    {
        public Func<GriddlyFilter, object> HtmlTemplate { get; set; }
    }
}