using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Griddly.Mvc
{
    public class GriddlyParameterAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext.ActionDescriptor.ActionName.EndsWith("grid", StringComparison.OrdinalIgnoreCase)
                    || ((ReflectedActionDescriptor)filterContext.ActionDescriptor).MethodInfo.ReturnType == typeof(GriddlyResult))
            {
                var request = filterContext.HttpContext.Request;
                var context = filterContext.Controller.GetOrCreateGriddlyContext();

                if (filterContext.IsChildAction)
                {
                    string[] parentKeys = request.QueryString.AllKeys;

                    // if a param came in on querystring, skip the defaults. cookie already does its own skip.
                    if (filterContext.ActionParameters.Any(x => x.Value != null && parentKeys.Contains(x.Key)))
                        context.IsDefaultSkipped = true;

                    foreach (var param in filterContext.ActionParameters.ToList())
                    {
                        if (param.Value != null)
                        {
                            // if we're skipping defaults but this didn't come from cookie or querystring, it must've come from a
                            // parameter default in code... nuke it.
                            if (context.IsDefaultSkipped && !(context.CookieData?.Values?.ContainsKey(param.Key) == true || parentKeys.Contains(param.Key)))
                                filterContext.ActionParameters[param.Key] = null;
                            else
                                context.Parameters[param.Key] = param.Value;
                        }
                    }
                }
             }

            base.OnActionExecuting(filterContext);
        }

        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            if (filterContext.Result is GriddlyResult result)
            {
                var context = filterContext.Controller.GetOrCreateGriddlyContext();
                var request = filterContext.HttpContext.Request;

                Uri parentPath = filterContext.IsChildAction ? request.Url : request.UrlReferrer;
                string parentPathString = parentPath?.PathAndQuery.Split('?')[0]; // TODO: less allocations than split

                if (parentPathString?.Length > 1)
                {
                    HttpCookie cookie = new HttpCookie("gf_" + context.Name)
                    {
                        Path = context.ParentPath
                    };

                    GriddlyFilterCookieData data = new GriddlyFilterCookieData()
                    {
                        Values = new Dictionary<string, string[]>()
                    };

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
                                var value = Extensions.GetFormattedValueByType(param.Value);

                                if (value != null)
                                    data.Values[param.Key] = value;
                            }
                        }
                    }

                    cookie.Value = JsonConvert.SerializeObject(data);

                    filterContext.HttpContext.Response.Cookies.Add(cookie);
                }
            }

            base.OnActionExecuted(filterContext);
        }
    }
}
