using System;

namespace Griddly.Mvc
{
    public class GriddlyHtmlFilter : GriddlyFilter
    {
        /// <summary>
        /// An Html Template function accepting a GriddlyHtmlFilterModel, and returning an object. 
        /// </summary>
        public Func<GriddlyHtmlFilterModel, object> HtmlTemplate { get; set; }
    }

    public class GriddlyHtmlFilterModel
    {
        public GriddlyHtmlFilterModel(GriddlyHtmlFilter filter, object defaultValue)
        {
            this.Filter = filter;
            this.DefaultValue = defaultValue;
        }

        public GriddlyHtmlFilter Filter { get; protected set; }

        public object DefaultValue { get; protected set; }
    }
}