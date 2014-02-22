using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Helpers;

namespace Griddly.Mvc
{
    public class SortField
    {
        public string Field { get; set; }
        public SortDirection Direction { get; set; }
    }
}
