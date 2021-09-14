using System;
using System.Collections.Generic;
using System.Text;
using System.Web;

#if NET45_OR_GREATER
using System.Web.Mvc;
#else
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
#endif

namespace Griddly.Mvc
{
    public interface IGriddlyColumnValueFilter
    {
        object Filter(GriddlyColumn column, object value,
#if NET45_OR_GREATER
            HttpContextBase httpContext
#else
            HttpContext httpContext
#endif
        );
    }
}
