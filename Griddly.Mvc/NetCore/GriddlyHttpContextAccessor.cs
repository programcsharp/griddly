using Microsoft.AspNetCore.Http;
using System.Threading;

namespace Griddly.Mvc;

public class GriddlyHttpContextAccessor : IHttpContextAccessor
{
    private static AsyncLocal<HttpContext> _httpContextCurrent = new AsyncLocal<HttpContext>();
    HttpContext IHttpContextAccessor.HttpContext { get => _httpContextCurrent.Value; set => _httpContextCurrent.Value = value; }
}
