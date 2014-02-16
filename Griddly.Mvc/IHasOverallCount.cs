using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Griddly.Mvc
{
    public interface IHasOverallCount
    {
        long OverallCount { get; set; }
    }
}