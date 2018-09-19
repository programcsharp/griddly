using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Griddly.Mvc
{
    public interface IGriddlyFilterSettings
    {
        List<GriddlyFilter> Filters { get; set; }
        Func<object, object> FilterButtonTemplate { get; set; }
    }
}
