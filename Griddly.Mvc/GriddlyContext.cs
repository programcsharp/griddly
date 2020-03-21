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
        public bool IsDefaultSkipped { get; set; }
        public bool IsDeepLink { get; set; }

        public GriddlyFilterCookieData CookieData { get; set; }

        public string CookieName => "gf_" + Name;

        public Dictionary<string, object> Defaults { get; set; } = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, object> Parameters { get; set; } = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public GriddlyExportFormat? ExportFormat { get; set; }
        public SortField[] SortFields { get; set; }
    }

    public class GriddlyFilterCookieData
    {
        public Dictionary<string, string[]> Values { get; set; }
        public SortField[] SortFields { get; set; }
        public DateTime? CreatedUtc { get; set; }
    }
}
