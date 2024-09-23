using Microsoft.AspNetCore.Http;
using System.Threading;

namespace Griddly.Mvc;

public class GriddlyHttpContextAccessor : IHttpContextAccessor
{
    private static AsyncLocal<HttpContext> _httpContextCurrent = new();
    HttpContext IHttpContextAccessor.HttpContext
    {
        get
        {
            try
            {
                var features = _httpContextCurrent.Value?.Features;
                return _httpContextCurrent.Value;
            }
            catch (ObjectDisposedException)
            {
                return null;
            }
        }
        set { _httpContextCurrent.Value = value; }
    }
}
