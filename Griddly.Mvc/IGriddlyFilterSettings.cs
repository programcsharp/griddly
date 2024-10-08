﻿namespace Griddly.Mvc;

public interface IGriddlyFilterSettings
{
    List<GriddlyFilter> Filters { get; set; }

#if NETFRAMEWORK
    Func<object, object> FilterButtonTemplate { get; set; }
        Func<IGriddlyFilterSettings, object> FilterModalHeaderTemplate { get; set; }
        Func<IGriddlyFilterSettings, object> FilterModalFooterTemplate { get; set; }
#else
    Func<object, IHtmlContent> FilterButtonTemplate { get; set; }
        Func<IGriddlyFilterSettings, IHtmlContent> FilterModalHeaderTemplate { get; set; }
        Func<IGriddlyFilterSettings, IHtmlContent> FilterModalFooterTemplate { get; set; }
#endif
}
