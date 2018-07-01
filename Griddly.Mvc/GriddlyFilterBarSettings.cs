using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.Routing;

namespace Griddly.Mvc
{
    public class GriddlyFilterBarSettings
    {
        public GriddlyFilterBarSettings()
        {
            Filters = new List<GriddlyFilter>();
        }

        public List<GriddlyFilter> Filters { get; set; }

        public GriddlyFilterBarSettings FilterBox(string field, string caption, FilterDataType dataType = FilterDataType.Decimal, string htmlClass = null, string captionPlural = null)
        {
            return Add(GriddlyFilterExtensions.FilterBox(null, dataType, field, caption, htmlClass, captionPlural));
        }

        public GriddlyFilterBarSettings FilterRange(string field, string fieldEnd, string caption, FilterDataType dataType = FilterDataType.Decimal, string htmlClass = null, string captionPlural = null)
        {
            return Add(GriddlyFilterExtensions.FilterRange(null, dataType, field, fieldEnd, caption, htmlClass, captionPlural));
        }

        public GriddlyFilterBarSettings FilterList(string field, string caption, IEnumerable<SelectListItem> items, bool isMultiple = true, bool defaultSelectAll = false, string nullItemText = null, bool isNoneAll = true, string htmlClass = null, string captionPlural = null, bool displayIncludeCaption = false)
        {
            return Add(GriddlyFilterExtensions.FilterList(null, items, isMultiple, defaultSelectAll, nullItemText, isNoneAll, field, caption, htmlClass, captionPlural, displayIncludeCaption));
        }

        public GriddlyFilterBarSettings FilterEnum<T>(string field, string caption, bool isMultiple = true, bool defaultSelectAll = false, string nullItemText = null, bool isNoneAll = true, string htmlClass = null, string captionPlural = null, bool displayIncludeCaption = false)
            where T : struct
        {
            return Add(GriddlyFilterExtensions.FilterEnum<T>(null, isMultiple, defaultSelectAll, nullItemText, isNoneAll, field, caption, htmlClass, captionPlural, displayIncludeCaption));
        }

        public GriddlyFilterBarSettings FilterEnum<T>(string field, string caption, IEnumerable<T> items, bool isMultiple = true, bool defaultSelectAll = false, string nullItemText = null, bool isNoneAll = true, string htmlClass = null, string captionPlural = null, bool displayIncludeCaption = false)
            where T : struct
        {
            return Add(GriddlyFilterExtensions.FilterEnum<T>(null, items, isMultiple, defaultSelectAll, nullItemText, isNoneAll, field, caption, htmlClass, captionPlural, displayIncludeCaption));
        }

        public GriddlyFilterBarSettings FilterBool(string field, string caption, string trueLabel = "Yes", string falseLabel = "No", string nullItemText = null, bool isMultiple = false, bool defaultSelectAll = false, bool isNoneAll = false, string htmlClass = null, string captionPlural = null, bool displayIncludeCaption = true)
        {
            return Add(GriddlyFilterExtensions.FilterBool(null, trueLabel, falseLabel, nullItemText, isMultiple, defaultSelectAll, isNoneAll, field, caption, htmlClass, captionPlural, displayIncludeCaption));
        }

        public GriddlyFilterBarSettings Add(GriddlyFilter filter)
        {
            Filters.Add(filter);

            return this;
        }
    }
}
