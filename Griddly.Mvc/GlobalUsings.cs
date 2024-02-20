global using System;
global using System.Collections;
global using System.Collections.Generic;
global using System.Collections.Specialized;
global using System.Linq;
global using System.Linq.Expressions;
global using System.Reflection;
global using System.Web;

#if NETFRAMEWORK
global using System.Web.Mvc;
global using System.Web.Helpers;
global using System.Web.Routing;
global using IHtmlHelper = System.Web.Mvc.HtmlHelper;
global using ActionContext = System.Web.Mvc.ControllerContext;
global using HttpMethod = System.Web.Mvc.HttpVerbs;
#else
global using System.Net.Http;
global using Microsoft.AspNetCore.Html;
global using Microsoft.AspNetCore.Mvc;
global using Microsoft.AspNetCore.Mvc.Rendering;
global using Microsoft.AspNetCore.Mvc.ViewFeatures;
global using Microsoft.AspNetCore.Routing;
global using HttpRequest = Microsoft.AspNetCore.Http.HttpRequest;
global using HttpContext = Microsoft.AspNetCore.Http.HttpContext;
global using HttpContextBase = Microsoft.AspNetCore.Http.HttpContext;
global using HelperResult = Microsoft.AspNetCore.Html.IHtmlContent;
#endif