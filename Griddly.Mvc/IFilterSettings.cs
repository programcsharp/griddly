using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#if !NET45_OR_GREATER
using Microsoft.AspNetCore.Html;
#endif

namespace Griddly.Mvc
{
    public interface IGriddlyFilterSettings
    {
        List<GriddlyFilter> Filters { get; set; }
#if NET45_OR_GREATER
        Func<object, object> FilterButtonTemplate { get; set; }
#else
        Func<object, IHtmlContent> FilterButtonTemplate { get; set; }
#endif
    }
}
