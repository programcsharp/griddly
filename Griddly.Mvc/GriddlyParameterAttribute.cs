using Newtonsoft.Json;
#if NETFRAMEWORK
using System.Web.Mvc.Async;
using COOKIE = System.Web.HttpCookie;
#else
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Net;
using COOKIE = System.Net.Cookie;
using Griddly.Mvc.InternalExtensions;
#endif

namespace Griddly.Mvc;

public class GriddlyParameterAttribute : ActionFilterAttribute
{
#if NETCOREAPP
    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        OnActionExecuting(context);
        var executedContext = await next(); //Execute the action
        await OnActionExecuted(executedContext);
    }
#endif

#if NETFRAMEWORK
    public override
#else
    private new 
#endif
    void OnActionExecuting(ActionExecutingContext filterContext)
    {
        if (filterContext.Controller is Controller controller)
        {
#if NETFRAMEWORK
            if (filterContext.ActionDescriptor.ActionName.EndsWith("grid", StringComparison.OrdinalIgnoreCase)
                    || typeof(GriddlyResult).IsAssignableFrom((filterContext.ActionDescriptor as ReflectedActionDescriptor)?.MethodInfo.ReturnType)
                    || typeof(GriddlyResult).IsAssignableFrom((filterContext.ActionDescriptor as TaskAsyncActionDescriptor)?.TaskMethodInfo.ReturnType))
#else
            if (filterContext.RouteData.Values["action"].ToString().EndsWith("grid", StringComparison.OrdinalIgnoreCase)
                    || typeof(GriddlyResult).IsAssignableFrom((filterContext.ActionDescriptor as ControllerActionDescriptor)?.MethodInfo.ReturnType))
#endif
            {
                var request = filterContext.HttpContext.Request;
                var context = controller.GetOrCreateGriddlyContext();

#if NETFRAMEWORK
                var actionArguments = new Dictionary<string, object>(filterContext.ActionParameters);
#else
                var actionArguments = new Dictionary<string, object>(filterContext.ActionArguments);

                //Add in the default parameter values
                foreach (var pd in filterContext.ActionDescriptor.Parameters.OfType<ControllerParameterDescriptor>())
                {
                    if (pd.ParameterInfo.HasDefaultValue && !actionArguments.ContainsKey(pd.Name))
                        actionArguments.Add(pd.Name, pd.ParameterInfo.DefaultValue);
                }
#endif

                //add any properties of model classes
                foreach (var ap in actionArguments.ToList().Where(x => x.Value?.GetType().IsClass == true && x.Value.GetType() != typeof(string))) 
                {
                    foreach (var pi in ap.Value.GetType().GetProperties().Where(x => x.CanRead && x.GetIndexParameters().Length == 0)) 
                    {
                        if (!actionArguments.ContainsKey(pi.Name))
                            actionArguments[pi.Name] = pi.GetValue(ap.Value);
                    } 
                }

                if (IsChildAction(filterContext))
                {
                    string[] querystringKeys = GetQueryStringKeys(request);

                    // if a param came in on querystring, skip the defaults. cookie already does its own skip.
                    if (!context.IsDefaultSkipped && actionArguments.Any(x => x.Value != null && querystringKeys.Contains(x.Key)))
                    {
                        context.IsDefaultSkipped = true;
                        context.IsDeepLink = true;
                    }

                    foreach (var param in actionArguments.ToList())
                    {
                        if (param.Value != null && (param.Value.GetType().IsValueType || typeof(IEnumerable).IsAssignableFrom(param.Value.GetType())))
                        {
                            bool isParamSet = context.CookieData?.Values?.ContainsKey(param.Key) == true || querystringKeys.Contains(param.Key) || filterContext.RouteData.Values.ContainsKey(param.Key);

                            if (isParamSet)
                            {
                                context.Parameters[param.Key] = param.Value;
                            }
                        }
                    }
                }
                else if (actionArguments.TryGetValue("_isDeepLink", out object value))
                {
                    if (Convert.ToBoolean(value) == true)
                        context.IsDeepLink = true;
                }
            }
        }
    }

#if NETFRAMEWORK
    public override void
#else
    new async Task
#endif
    OnActionExecuted(ActionExecutedContext filterContext)
    {
        if (filterContext.Result is GriddlyResult result && filterContext.Controller is Controller controller)
        {
            var context = controller.GetOrCreateGriddlyContext();

            if (!context.IsDeepLink)
            {
                var request = filterContext.HttpContext.Request;

#if NETFRAMEWORK
                Uri parentPath = filterContext.IsChildAction ? request.Url : request.UrlReferrer;
                string parentPathString = parentPath?.PathAndQuery.Split('?')[0]; // TODO: less allocations than split
#else
                var referrer = request.Headers["Referer"].ToString();
                string parentPathString = filterContext.HttpContext.IsChildAction() ? request.Path.ToString() :
                    !string.IsNullOrWhiteSpace(referrer) ? new Uri(referrer).PathAndQuery.Split("?")[0] : null;
#endif

                if (parentPathString?.Length > 0)
                {
                    GriddlyFilterCookieData data = new GriddlyFilterCookieData()
                    {
                        Values = new Dictionary<string, string[]>(),
                        CreatedUtc = DateTime.UtcNow
                    };

                    // now, we could use the context.Parameters... but the raw string values seems more like what we want here...
#if NETFRAMEWORK
                    foreach (var param in filterContext.ActionDescriptor.GetParameters())
                    {
                        if (param.ParameterType.IsClass && !param.ParameterType.IsArray && param.ParameterType != typeof(string))
                        {
                            foreach (var pi in param.ParameterType.GetProperties().Where(x => x.CanRead && x.GetIndexParameters().Length == 0))
                                AddParameter(filterContext, data, pi.Name);
                        }
                        else
                        {
                            AddParameter(filterContext, data, param.ParameterName);
                        }
                    }
#else
                    var valueProvider = await CompositeValueProvider.CreateAsync(controller.ControllerContext);
                    foreach (var param in filterContext.ActionDescriptor.Parameters)
                    {
                        var name = param.Name;

                        if (param.ParameterType.IsClass && !param.ParameterType.IsArray && param.ParameterType != typeof(string))
                        {
                            foreach (var pi in param.ParameterType.GetProperties().Where(x => x.CanRead && x.GetIndexParameters().Length == 0))
                                AddParameter(valueProvider, data, pi.Name);
                        }
                        else
                        {
                            AddParameter(valueProvider, data, name);
                        }
                    }
#endif

                    // ... but if it is a defaults situation, we actually need to grab those
                    if (IsChildAction(filterContext) && !context.IsDefaultSkipped && context.Defaults.Count > 0)
                    {
                        foreach (var param in context.Defaults)
                        {
                            if (param.Value != null)
                            {
                                var value = GriddlyExtensions.GetFormattedValueByType(param.Value);

                                if (value != null)
                                    data.Values[param.Key] = value;
                            }
                        }
                    }

                    string cookieName = "gf_" + context.Name;
#if NETFRAMEWORK
                    HttpCookie cookie = new HttpCookie(cookieName)
                    {
                        Path = parentPathString
                    };
#else
                    Cookie cookie = new Cookie(cookieName, null, parentPathString);
#endif
                    filterContext.HttpContext.Items["_griddlyCookie"] = cookie;
                    filterContext.HttpContext.Items["_griddlyCookieData"] = data;
                }
            }
        }
    }

    public static void AddCookieDataIfNeeded(GriddlyContext context, HttpContextBase httpContext)
    {
#if NETFRAMEWORK
        if (!GriddlySettings.IsCookiesDisabled(httpContext))
#else
        if (!httpContext.GetGriddlyConfig().IsCookiesDisabled())
#endif
        {
            var cookie = (COOKIE)httpContext.Items["_griddlyCookie"];
            var data = (GriddlyFilterCookieData)httpContext.Items["_griddlyCookieData"];

            if (cookie != null && data != null)
            {
                // we have to use the whitelisted hunk
                if (context.SortFields?.Length > 0)
                    data.SortFields = context.SortFields;

                cookie.Name = context.CookieName; // cookie name could be different than the default
                cookie.Value = JsonConvert.SerializeObject(data);

#if NETFRAMEWORK
            httpContext.Response.Cookies.Add(cookie);
#else
                httpContext.Response.Cookies.Append(cookie.Name, cookie.Value, new CookieOptions() { Path = cookie.Path });
#endif
            }
        }
    }

#if NETFRAMEWORK
    void AddParameter(ActionExecutedContext filterContext, GriddlyFilterCookieData data, string parameterName)
    {
        var valueResult = filterContext.Controller.ValueProvider.GetValue(parameterName);

        if (valueResult?.RawValue != null)
        {
            if (valueResult.RawValue is string[] array)
                data.Values[parameterName] = array;
            else
                data.Values[parameterName] = new[] { valueResult.RawValue.ToString() };
        }
    }
#else
    void AddParameter(IValueProvider valueProvider, GriddlyFilterCookieData data, string parameterName)
    {
        var valueResult = valueProvider.GetValue(parameterName);
        data.Values[parameterName] = valueResult.Values.ToArray();
    }
#endif

    bool IsChildAction(ActionExecutingContext context) =>
#if NETFRAMEWORK
        context.IsChildAction;
#else
        context.HttpContext.IsChildAction();
#endif

    bool IsChildAction(ActionExecutedContext context) =>
#if NETFRAMEWORK
        context.IsChildAction;
#else
        context.HttpContext.IsChildAction();
#endif

#if NETFRAMEWORK
    string[] GetQueryStringKeys(HttpRequestBase request) => request.QueryString.AllKeys;
#else
    string[] GetQueryStringKeys(HttpRequest request) => request.Query.Keys.ToArray();
#endif
}
