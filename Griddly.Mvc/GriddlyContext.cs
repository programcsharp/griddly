using System;
using System.Collections.Generic;

namespace Griddly.Mvc;

public class GriddlyContext
{
    string _name;
    string _cookieName;

    public string Name
    {
        get => _name;
        set
        {
            _name = value;

            _cookieName = "gf_" + Name;

            if (GriddlySettings.DecorateCookieName != null)
                _cookieName = GriddlySettings.DecorateCookieName(_cookieName);
        }
    }

    public bool IsDefaultSkipped { get; set; }
    public bool IsDeepLink { get; set; }

    public GriddlyFilterCookieData CookieData { get; set; }

    public string CookieName => _cookieName;

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
