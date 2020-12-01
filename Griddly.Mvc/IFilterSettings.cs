using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#if !NET45
using Microsoft.AspNetCore.Html;
#endif

namespace Griddly.Mvc
{
    public interface IGriddlyFilterSettings
    {
        List<GriddlyFilter> Filters { get; set; }
#if NET45
        Func<object, object> FilterButtonTemplate { get; set; }
#else
        Func<object, IHtmlContent> FilterButtonTemplate { get; set; }
#endif
    }
}
