using System.Threading.Tasks;

namespace Griddly.Mvc;

public interface IGriddlyFilterSettings
{
    List<GriddlyFilter> Filters { get; set; }

#if NETFRAMEWORK
    Func<object, object> FilterButtonTemplate { get; set; }
    Func<IGriddlyFilterSettings, object> FilterModalHeaderTemplate { get; set; }
    Func<IGriddlyFilterSettings, object> FilterModalFooterTemplate { get; set; }
#else
    Func<IHtmlHelper, Task<IHtmlContent>> FilterButtonTemplate { get; set; }
    Func<IGriddlyFilterSettings, IHtmlHelper, Task<IHtmlContent>> FilterModalHeaderTemplate { get; set; }
    Func<IGriddlyFilterSettings, IHtmlHelper, Task<IHtmlContent>> FilterModalFooterTemplate { get; set; }
#endif
}