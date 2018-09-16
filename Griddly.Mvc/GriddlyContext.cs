using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Griddly.Mvc
{
    public class GriddlyContext
    {
        public string Name { get; set; }
        public string ParentPath { get; set; }
        public bool IsDefaultSkipped { get; set; }

        public GriddlyFilterCookieData CookieData { get; set; }

        public string CookieName => "gf_" + Name;

        public Dictionary<string, object> Defaults { get; set; } = new Dictionary<string, object>();
        public Dictionary<string, object> Parameters { get; set; } = new Dictionary<string, object>();
    }

    public class GriddlyFilterCookieData
    {
        public Dictionary<string, string[]> Values { get; set; }
    }
}
