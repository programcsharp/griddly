using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#if NETCOREAPP
using Microsoft.AspNetCore.Html;
#endif

namespace Griddly.Mvc
{
    public interface IGriddlyFilterSettings
    {
        List<GriddlyFilter> Filters { get; set; }
#if NETFRAMEWORK
        Func<object, object> FilterButtonTemplate { get; set; }
#else
        Func<object, IHtmlContent> FilterButtonTemplate { get; set; }
#endif
    }
}
