using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
#if NET45
using System.Web.Mvc;
using System.Web.Mvc.Async;
#else
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Net;
#endif

namespace Griddly.Mvc
{
    public class GriddlyParameterAttribute : ActionFilterAttribute
    {
        public static bool DefaultIgnoreSkipped { get; set; } = true;

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
#if NET45
            if (filterContext.ActionDescriptor.ActionName.EndsWith("grid", StringComparison.OrdinalIgnoreCase)
                    || (filterContext.ActionDescriptor as ReflectedActionDescriptor)?.MethodInfo.ReturnType == typeof(GriddlyResult)
                    || (filterContext.ActionDescriptor as TaskAsyncActionDescriptor)?.TaskMethodInfo.ReturnType == typeof(GriddlyResult))
#else
            if (filterContext.Controller is Controller) //TODO: implement
#endif
            { 
                var request = filterContext.HttpContext.Request;

#if (NET45)
                var context = filterContext.Controller.GetOrCreateGriddlyContext();
                var args = filterContext.ActionParameters;
                if (filterContext.IsChildAction)
                {
                    string[] parentKeys = request.QueryString.AllKeys;
#else
                var context = (filterContext.Controller as Controller).GetOrCreateGriddlyContext();
                var args = filterContext.ActionArguments;
                if (filterContext.HttpContext.IsChildAction())
                {
                    string[] parentKeys = request.Query.Keys.ToArray();
#endif

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

            base.OnActionExecuting(filterContext);
        }

        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            if (filterContext.Result is GriddlyResult result)
            {
#if NET45
                var context = filterContext.Controller.GetOrCreateGriddlyContext();
#else
                var context = (filterContext.Controller as Controller).GetOrCreateGriddlyContext();
#endif

                if (!context.IsDeepLink)
                {
                    var request = filterContext.HttpContext.Request;

#if NET45
                    Uri parentPath = filterContext.IsChildAction ? request.Url : request.UrlReferrer;
                    string parentPathString = parentPath?.PathAndQuery.Split('?')[0]; // TODO: less allocations than split
#else
                    var referrer = request.Headers["Referrer"].ToString();
                    string parentPathString = filterContext.HttpContext.IsChildAction() ? request.Path.ToString() :
                       referrer != null ? new Uri(referrer).PathAndQuery.Split("?")[0] : null;
#endif

                    if (parentPathString?.Length > 0)
                    {
                        GriddlyFilterCookieData data = new GriddlyFilterCookieData()
                        {
                            Values = new Dictionary<string, string[]>(),
                            CreatedUtc = DateTime.UtcNow
                        };

                        if (context.SortFields?.Length > 0)
                            data.SortFields = context.SortFields;

#if NET45
                        // now, we could use the context.Parameters... but the raw string values seems more like what we want here...
                        foreach (var param in filterContext.ActionDescriptor.GetParameters())
                        {
                            var valueResult = filterContext.Controller.ValueProvider.GetValue(param.ParameterName);

                            if (valueResult?.RawValue != null)
                            {
                                if (valueResult.RawValue is string[] array)
                                    data.Values[param.ParameterName] = array;
                                else
                                    data.Values[param.ParameterName] = new[] { valueResult.RawValue.ToString() };
                            }
                        }
#else
                        //TODO: implement
#endif

                        // ... but if it is a defaults situation, we actually need to grab those
#if NET45
                        if (filterContext.IsChildAction && !context.IsDefaultSkipped && context.Defaults.Count > 0)
#else
                        if (filterContext.HttpContext.IsChildAction() && !context.IsDefaultSkipped && context.Defaults.Count > 0)
#endif
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
                        var cookieValue = JsonConvert.SerializeObject(data);

#if NET45
                        HttpCookie cookie = new HttpCookie("gf_" + context.Name)
                        {
                            Path = parentPathString,
                            Value = cookieValue
                        };
                        filterContext.HttpContext.Response.Cookies.Add(cookie);
#else
                        filterContext.HttpContext.Response.Cookies.Append("gf_" + context.Name, cookieValue, new CookieOptions() { Path = parentPathString });
#endif
                        
                    }
                }
            }

            base.OnActionExecuted(filterContext);
        }
    }
}
