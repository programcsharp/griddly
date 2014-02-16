using System;

namespace Griddly.Mvc
{
    public class GriddlyHtmlButton : GriddlyButton
    {
        public Func<object, object> HtmlTemplate { get; set; }
    }
}