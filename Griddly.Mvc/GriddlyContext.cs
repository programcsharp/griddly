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

    /// <summary>
    /// Indicates that the <see cref="Defaults"/> have been initialized from a different source than SetGriddlyDefault
    /// </summary>
    public bool IsDefaultReplaced { get; set; }

    /// <summary>
    /// Indicates that the <see cref="Defaults"/> should not be used for filtering since <see cref="Parameters"/> have been initialized from another source
    /// </summary>
    public bool IsDefaultSkipped { get; set; }

    /// <summary>
    /// Indicates that the <see cref="Parameters"/> have been set by a querystring values
    /// </summary>
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
