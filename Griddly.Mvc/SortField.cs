#if NET45
using System.Web.Helpers;
#endif

namespace Griddly.Mvc
{
    public class SortField
    {
        public string Field { get; set; }
        public SortDirection Direction { get; set; }
    }
}
