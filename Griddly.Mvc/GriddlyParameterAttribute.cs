using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using OfficeOpenXml.ConditionalFormatting;
#if NET45_OR_GREATER
using System.Web.Mvc;
using System.Web.Mvc.Async;
using COOKIE = System.Web.HttpCookie;
#else
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Net;
using COOKIE = System.Net.Cookie;
#endif

namespace Griddly.Mvc
{
    public class GriddlyParameterAttribute : ActionFilterAttribute
    {
        public static bool DefaultIgnoreSkipped { get; set; } = true;


#if !NET45_OR_GREATER
        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            OnActionExecuting(context);
            var executedContext = await next(); //Execute the action
            await OnActionExecuted(executedContext);
        }
#endif

#if NET45_OR_GREATER
        public override
#else
        private new 
#endif
        void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext.Controller is Controller controller)
            {
#if NET45_OR_GREATER
                if (filterContext.ActionDescriptor.ActionName.EndsWith("grid", StringComparison.OrdinalIgnoreCase)
                    || (filterContext.ActionDescriptor as ReflectedActionDescriptor)?.MethodInfo.ReturnType == typeof(GriddlyResult)
                    || (filterContext.ActionDescriptor as TaskAsyncActionDescriptor)?.TaskMethodInfo.ReturnType == typeof(GriddlyResult))
#else
                if (filterContext.RouteData.Values["action"].ToString().EndsWith("grid", StringComparison.OrdinalIgnoreCase)
                    || (filterContext.ActionDescriptor as ControllerActionDescriptor)?.MethodInfo.ReturnType == typeof(GriddlyResult))
#endif
                {
                    var request = filterContext.HttpContext.Request;
                    var context = controller.GetOrCreateGriddlyContext();
#if NET45_OR_GREATER
                    var args = new Dictionary<string, object>(filterContext.ActionParameters);
#else
                    var args = new Dictionary<string, object>(filterContext.ActionArguments);
                    //Add in the default parameter values
                    foreach (var pd in filterContext.ActionDescriptor.Parameters.OfType<ControllerParameterDescriptor>())
                    {
                        if (pd.ParameterInfo.HasDefaultValue && !args.ContainsKey(pd.Name))
                            args.Add(pd.Name, pd.ParameterInfo.DefaultValue);
                    }
#endif

                    //add any properties of model classes
                    foreach (var ap in args.ToList().Where(x => x.Value?.GetType().IsClass == true && x.Value.GetType() != typeof(string))) 
                    {
                        foreach (var pi in ap.Value.GetType().GetProperties().Where(x => x.CanRead && x.GetIndexParameters().Length == 0)) 
                        {
                            if (!args.ContainsKey(pi.Name))
                                args[pi.Name] = pi.GetValue(ap.Value);
                        } 
                    }

                    if (IsChildAction(filterContext))
                    {
                        string[] parentKeys = GetQueryStringKeys(request);

                        // if a param came in on querystring, skip the defaults. cookie already does its own skip.
                        if (!context.IsDefaultSkipped && args.Any(x => x.Value != null && parentKeys.Contains(x.Key)))
                        {
                            context.IsDefaultSkipped = true;
                            context.IsDeepLink = true;
                        }

                        foreach (var param in args.ToList())
                        {
                            if (param.Value != null && (param.Value.GetType().IsValueType || typeof(IEnumerable).IsAssignableFrom(param.Value.GetType())))
                            {
                                bool isParamSet = context.CookieData?.Values?.ContainsKey(param.Key) == true || parentKeys.Contains(param.Key) || filterContext.RouteData.Values.ContainsKey(param.Key);

                                // if we're skipping defaults but this didn't come from cookie or querystring, it must've come from a
                                // parameter default in code... nuke it.
                                if ((context.IsDefaultSkipped && !DefaultIgnoreSkipped) && !isParamSet)
                                    args[param.Key] = null;
                                else
                                {
                                    if (!(context.IsDefaultSkipped && !DefaultIgnoreSkipped) || !isParamSet)
                                        context.Defaults[param.Key] = param.Value;

                                    context.Parameters[param.Key] = param.Value;
                                }
                            }
                        }
                    }
                    else if (args.TryGetValue("_isDeepLink", out object value))
                    {
                        if (Convert.ToBoolean(value) == true)
                            context.IsDeepLink = true;
                    }
                }
            }
        }

#if NET45_OR_GREATER
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

#if NET45_OR_GREATER
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
#if NET45_OR_GREATER
                        foreach (var param in filterContext.ActionDescriptor.GetParameters())
                        {
                            if (param.ParameterType.IsClass && param.ParameterType != typeof(string))
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

                            if (param.ParameterType.IsClass && param.ParameterType != typeof(string))
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
#if NET45_OR_GREATER
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

#if NET45_OR_GREATER
        public static void AddCookieDataIfNeeded(GriddlyContext context, HttpContextBase httpContext)
#else
        public static void AddCookieDataIfNeeded(GriddlyContext context, HttpContext httpContext)
#endif
        {
            var cookie = (COOKIE)httpContext.Items["_griddlyCookie"];
            var data = (GriddlyFilterCookieData)httpContext.Items["_griddlyCookieData"];

            if (cookie != null && data != null)
            {
                // we have to use the whitelisted hunk
                if (context.SortFields?.Length > 0)
                    data.SortFields = context.SortFields;

                cookie.Value = JsonConvert.SerializeObject(data);

#if NET45_OR_GREATER
                httpContext.Response.Cookies.Add(cookie);
#else
                httpContext.Response.Cookies.Append(cookie.Name, cookie.Value, new CookieOptions() { Path = cookie.Path });
#endif
            }
        }

#if NET45_OR_GREATER
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
#if NET45_OR_GREATER
            context.IsChildAction;
#else
            context.HttpContext.IsChildAction();
#endif

        bool IsChildAction(ActionExecutedContext context) =>
#if NET45_OR_GREATER
            context.IsChildAction;
#else
            context.HttpContext.IsChildAction();
#endif

#if NET45_OR_GREATER
        string[] GetQueryStringKeys(HttpRequestBase request) => request.QueryString.AllKeys;
#else
        string[] GetQueryStringKeys(HttpRequest request) => request.Query.Keys.ToArray();
#endif
    }
}
