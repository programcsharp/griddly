namespace Griddly.Mvc;

public class GriddlyFilterBarSettings : IGriddlyFilterSettings
{
    public List<GriddlyFilter> Filters { get; set; } = new List<GriddlyFilter>();

#if NETFRAMEWORK
    public Func<object, object> FilterButtonTemplate { get; set; }
        public Func<IGriddlyFilterSettings, object> FilterModalHeaderTemplate { get; set; }
        public Func<IGriddlyFilterSettings, object> FilterModalFooterTemplate { get; set; }
#else
    public Func<object, IHtmlContent> FilterButtonTemplate { get; set; }
        public Func<IGriddlyFilterSettings, IHtmlContent> FilterModalHeaderTemplate { get; set; }
        public Func<IGriddlyFilterSettings, IHtmlContent> FilterModalFooterTemplate { get; set; }
#endif


    public GriddlyFilterBarSettings FilterBox(string field, string caption, FilterDataType dataType = FilterDataType.Decimal, string htmlClass = null, string captionPlural = null, string group = null, object inputHtmlAttributes = null)
    {
        return Add(GriddlyFilterExtensions.FilterBox(null, dataType, field, caption, htmlClass, captionPlural, group, inputHtmlAttributes: inputHtmlAttributes));
    }

    public GriddlyFilterBarSettings FilterRange(string field, string fieldEnd, string caption, FilterDataType dataType = FilterDataType.Decimal, string htmlClass = null, string captionPlural = null, string group = null, object inputHtmlAttributes = null)
    {
        return Add(GriddlyFilterExtensions.FilterRange(null, dataType, field, fieldEnd, caption, htmlClass, captionPlural, group, inputHtmlAttributes: inputHtmlAttributes));
    }

    public GriddlyFilterBarSettings FilterList(string field, string caption, IEnumerable<SelectListItem> items, bool isMultiple = true, bool defaultSelectAll = false, string nullItemText = null, bool isNoneAll = true, string htmlClass = null, string captionPlural = null, bool displayIncludeCaption = false, string group = null, object inputHtmlAttributes = null)
    {
        return Add(GriddlyFilterExtensions.FilterList(null, items, isMultiple, defaultSelectAll, nullItemText, isNoneAll, field, caption, htmlClass, captionPlural, displayIncludeCaption, group, inputHtmlAttributes: inputHtmlAttributes));
    }

    public GriddlyFilterBarSettings FilterEnum<T>(string field, string caption, bool isMultiple = true, bool defaultSelectAll = false, string nullItemText = null, bool isNoneAll = true, string htmlClass = null, string captionPlural = null, bool displayIncludeCaption = false, string group = null, object inputHtmlAttributes = null)
        where T : struct
    {
        return Add(GriddlyFilterExtensions.FilterEnum<T>(null, isMultiple, defaultSelectAll, nullItemText, isNoneAll, field, caption, htmlClass, captionPlural, displayIncludeCaption, group, inputHtmlAttributes: inputHtmlAttributes));
    }

    public GriddlyFilterBarSettings FilterEnum<T>(string field, string caption, IEnumerable<T> items, bool isMultiple = true, bool defaultSelectAll = false, string nullItemText = null, bool isNoneAll = true, string htmlClass = null, string captionPlural = null, bool displayIncludeCaption = false, string group = null, bool sort = true, object inputHtmlAttributes = null)
        where T : struct
    {
        return Add(GriddlyFilterExtensions.FilterEnum<T>(null, items, isMultiple, defaultSelectAll, nullItemText, isNoneAll, field, caption, htmlClass, captionPlural, displayIncludeCaption, group, sort: sort, inputHtmlAttributes: inputHtmlAttributes));
    }

    public GriddlyFilterBarSettings FilterBool(string field, string caption, string trueLabel = "Yes", string falseLabel = "No", string nullItemText = null, bool isMultiple = false, bool defaultSelectAll = false, bool isNoneAll = false, string htmlClass = null, string captionPlural = null, bool displayIncludeCaption = true, string group = null, object inputHtmlAttributes = null)
    {
        return Add(GriddlyFilterExtensions.FilterBool(null, trueLabel, falseLabel, nullItemText, isMultiple, defaultSelectAll, isNoneAll, field, caption, htmlClass, captionPlural, displayIncludeCaption, group, inputHtmlAttributes: inputHtmlAttributes));
    }

    public GriddlyFilterBarSettings Add(GriddlyFilter filter)
    {
        Filters.Add(filter);

        return this;
    }
}
