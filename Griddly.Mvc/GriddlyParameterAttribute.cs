using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Async;

namespace Griddly.Mvc
{
    public class GriddlyParameterAttribute : ActionFilterAttribute
    {
        public static bool DefaultIgnoreSkipped { get; set; } = true;

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext.ActionDescriptor.ActionName.EndsWith("grid", StringComparison.OrdinalIgnoreCase)
                    || (filterContext.ActionDescriptor as ReflectedActionDescriptor)?.MethodInfo.ReturnType == typeof(GriddlyResult)
                    || (filterContext.ActionDescriptor as TaskAsyncActionDescriptor)?.TaskMethodInfo.ReturnType == typeof(GriddlyResult))
            {
                var request = filterContext.HttpContext.Request;
                var context = filterContext.Controller.GetOrCreateGriddlyContext();

                if (filterContext.IsChildAction)
                {
                    string[] parentKeys = request.QueryString.AllKeys;

                    // if a param came in on querystring, skip the defaults. cookie already does its own skip.
                    if (!context.IsDefaultSkipped && filterContext.ActionParameters.Any(x => x.Value != null && parentKeys.Contains(x.Key)))
                    {
                        context.IsDefaultSkipped = true;
                        context.IsDeepLink = true;
                    }

                    foreach (var param in filterContext.ActionParameters.ToList())
                    {
                        if (param.Value != null && (param.Value.GetType().IsValueType || typeof(IEnumerable).IsAssignableFrom(param.Value.GetType())))
                        {
                            bool isParamSet = context.CookieData?.Values?.ContainsKey(param.Key) == true || parentKeys.Contains(param.Key) || filterContext.RouteData.Values.ContainsKey(param.Key);

                            // if we're skipping defaults but this didn't come from cookie or querystring, it must've come from a
                            // parameter default in code... nuke it.
                            if ((context.IsDefaultSkipped && !DefaultIgnoreSkipped) && !isParamSet)
                                filterContext.ActionParameters[param.Key] = null;
                            else
                            {
                                if (!(context.IsDefaultSkipped && !DefaultIgnoreSkipped) || !isParamSet)
                                    context.Defaults[param.Key] = param.Value;

                                context.Parameters[param.Key] = param.Value;
                            }
                        }
                    }
                }
                else if (filterContext.ActionParameters.TryGetValue("_isDeepLink", out object value))
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
                var context = filterContext.Controller.GetOrCreateGriddlyContext();

                if (!context.IsDeepLink)
                {
                    var request = filterContext.HttpContext.Request;

                    Uri parentPath = filterContext.IsChildAction ? request.Url : request.UrlReferrer;
                    string parentPathString = parentPath?.PathAndQuery.Split('?')[0]; // TODO: less allocations than split

                    if (parentPathString?.Length > 0)
                    {
                        HttpCookie cookie = new HttpCookie("gf_" + context.Name)
                        {
                            Path = parentPathString
                        };

                        GriddlyFilterCookieData data = new GriddlyFilterCookieData()
                        {
                            Values = new Dictionary<string, string[]>(),
                            CreatedUtc = DateTime.UtcNow
                        };

                        if (context.SortFields?.Length > 0)
                            data.SortFields = context.SortFields;

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

                        // ... but if it is a defaults situation, we actually need to grab those
                        if (filterContext.IsChildAction && !context.IsDefaultSkipped && context.Defaults.Count > 0)
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

                        cookie.Value = JsonConvert.SerializeObject(data);

                        filterContext.HttpContext.Response.Cookies.Add(cookie);
                    }
                }
            }

            base.OnActionExecuted(filterContext);
        }
    }
}
