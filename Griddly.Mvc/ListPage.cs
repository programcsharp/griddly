using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Griddly.Mvc
{
    public class ListPage<T> : List<T>, IHasOverallCount
    {
        public long OverallCount { get; set; }
    }
}
