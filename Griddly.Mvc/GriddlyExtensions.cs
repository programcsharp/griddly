using Griddly.Mvc.Linq.Dynamic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Web.Routing;
using System.Web.WebPages;
using System.Web.WebPages.Instrumentation;

namespace Griddly.Mvc
{
    public static class GriddlyExtensions
    {
        public static string CurrencySymbol
        {
            get
            {
                return Thread.CurrentThread.CurrentCulture.NumberFormat.CurrencySymbol;
            }
        }

        public static MvcHtmlString Griddly(this HtmlHelper htmlHelper, string actionName)
        {
            return htmlHelper.Griddly(actionName, null);
        }

        public static MvcHtmlString Griddly(this HtmlHelper htmlHelper, string actionName, object routeValues)
        {
            return htmlHelper.Griddly(actionName, null, routeValues);
        }

        public static MvcHtmlString Griddly(this HtmlHelper htmlHelper, string actionName, string controllerName)
        {
            return htmlHelper.Griddly(actionName, controllerName, null);
        }

        public static MvcHtmlString Griddly(this HtmlHelper htmlHelper, string actionName, string controllerName, object routeValues)
        {
            // TODO: validate that we got a GriddlyResult
            return htmlHelper.Action(actionName, controllerName, routeValues);
        }

        public static MvcHtmlString Griddly(this HtmlHelper htmlHelper, GriddlySettings settings)
        {
            return htmlHelper.Griddly((GriddlyResultPage)htmlHelper.ViewData.Model, settings);
        }

        public static MvcHtmlString SimpleGriddly<T>(this HtmlHelper htmlHelper, GriddlySettings<T> settings, IEnumerable<T> data)
        {
            // TODO: figure out how to get this in one query
            foreach (GriddlyColumn c in settings.Columns.Where(x => x.SummaryFunction != null))
                PopulateSummaryValue(data, c);

            return htmlHelper.Griddly(new GriddlyResultPage<T>(data), settings, true);
        }

        static void PopulateSummaryValue<T>(IEnumerable<T> data, GriddlyColumn c)
        {
            // NOTE: Also in QueryableResult.PopulateSummaryValue
            switch (c.SummaryFunction.Value)
            {
                case SummaryAggregateFunction.Sum:
                case SummaryAggregateFunction.Average:
                case SummaryAggregateFunction.Min:
                case SummaryAggregateFunction.Max:
                    c.SummaryValue = data.AsQueryable().Aggregate(c.SummaryFunction.Value.ToString(), c.ExpressionString);

                    break;

                default:
                    throw new InvalidOperationException(string.Format("Unknown summary function {0} for column {1}.", c.SummaryFunction, c.ExpressionString));
            }
        }

        public static MvcHtmlString Griddly(this HtmlHelper htmlHelper, GriddlyResultPage model, GriddlySettings settings, bool isSimpleGriddly = false)
        {
            if (htmlHelper.ViewData["_isGriddlySettingsRequest"] as bool? != true)
            {
                ViewDataDictionary viewData = new ViewDataDictionary(htmlHelper.ViewData);

                viewData.Model = model;

                viewData["settings"] = settings;
                viewData["isSimpleGriddly"] = isSimpleGriddly;

                return htmlHelper.Partial("~/Views/Shared/Griddly/Griddly.cshtml", viewData);
            }
            else
            {
                htmlHelper.ViewContext.ViewData["settings"] = settings;

                return null;
            }
        }

        public static MvcHtmlString GriddlyFilterBar(this HtmlHelper htmlHelper, GriddlyFilterBarSettings settings)
        {
            return htmlHelper.Partial("~/Views/Shared/Griddly/GriddlyFilterBar.cshtml", settings);
        }

        public static GriddlyColumn<TRow> GriddlyColumnFor<TRow>(this HtmlHelper<IEnumerable<TRow>> htmlHelper, Func<TRow, object> template)
        {
            return htmlHelper.GriddlyColumnFor<TRow>(template, null);
        }

        public static GriddlyColumn<TRow> GriddlyColumnFor<TRow>(this HtmlHelper<IEnumerable<TRow>> htmlHelper, Func<TRow, object> template, string caption)
        {
            return new GriddlyColumn<TRow>()
            {
                Caption = caption,
                Template = template
            };
        }

        public static HtmlString AttributeNullable(this HtmlHelper helper, string name, string value)
        {
            if (value == null)
                return null;
            else
                return new HtmlString(name + "=\"" + helper.Encode(value) + "\"");
        }

        public static HtmlString AttributeIf(this HtmlHelper helper, string name, bool shouldShow, Func<object, object> value)
        {
            if (shouldShow)
                return helper.AttributeIf(name, shouldShow, value(null));
            else
                return null;
        }

        public static HtmlString AttributeIf(this HtmlHelper helper, string name, bool shouldShow, object value)
        {
            if (shouldShow)
                return new HtmlString(name + "=\"" + value + "\"");
            else
                return null;
        }

        // http://stackoverflow.com/a/18618808/8037
        public static IHtmlString ToHtmlAttributes(this IDictionary<string, object> dictionary)
        {
            if (dictionary == null || dictionary.Count == 0)
                return null;

            var sb = new StringBuilder();

            foreach (var kvp in dictionary)
            {
                sb.Append(string.Format("{0}=\"{1}\" ", HttpUtility.HtmlEncode(kvp.Key), HttpUtility.HtmlAttributeEncode(kvp.Value != null ? kvp.Value.ToString() : null)));
            }

            return new HtmlString(sb.ToString());
        }

        static readonly string _contextKey = "_griddlycontext";

        public static void SetGriddlyDefault<T>(this ControllerBase controller, ref T parameter, string field, T value)
        {
            var context = controller.ViewData[_contextKey] as GriddlyContext;

            if (context != null)
            {
                context.Defaults[field] = value;

                if (controller.ControllerContext.IsChildAction
                    && !context.IsDefaultSkipped
                    && EqualityComparer<T>.Default.Equals(parameter, default(T)))
                {
                    parameter = value;

                    context.Parameters[field] = value;
                }
            }
        }

        public static void SetGriddlyDefault<T>(this ControllerBase controller, ref T[] parameter, string field, IEnumerable<T> value)
        {
            var context = controller.ViewData[_contextKey] as GriddlyContext;

            if (context != null)
            {
                context.Defaults[field] = value;

                if (controller.ControllerContext.IsChildAction
                    && !context.IsDefaultSkipped
                    && parameter == null)
                {
                    parameter = value.ToArray();
                }
            }
        }

        public static void SetGriddlyDefault<T>(this ControllerBase controller, ref T?[] parameter, string field, IEnumerable<T> value)
            where T : struct
        {
            var context = controller.ViewData[_contextKey] as GriddlyContext;

            if (context != null)
            {
                context.Defaults[field] = value;

                if (controller.ControllerContext.IsChildAction
                    && !context.IsDefaultSkipped
                    && parameter == null)
                {
                    parameter = value.Cast<T?>().ToArray();
                }
            }
        }

        public static object GetGriddlyParameter(this WebViewPage page, string field)
        {
            object value = null;

            if ((page.ViewData[_contextKey] as GriddlyContext)?.Parameters.TryGetValue(field, out value) != true)
                value = null;

            return value;
        }

        public static Dictionary<string, object> GetGriddlyDefaults(this WebViewPage page)
        {
            Dictionary<string, object> defaults = new Dictionary<string, object>();
            var context = page.ViewData[_contextKey] as GriddlyContext;

            if (context != null)
            {
                // TODO: is there any reason to make a new dict vs using the same one? nobody else uses it, right?
                foreach (var pair in context.Defaults)
                {
                    var value = pair.Value;

                    if (value != null)
                    {
                        Type t = value.GetType();

                        if (t.IsArray)
                        {
                            t = t.GetElementType();

                            if ((Nullable.GetUnderlyingType(t) ?? t).IsEnum)
                                value = ((Array)value).Cast<object>().Select(x => x?.ToString()).ToArray();
                        }
                    }

                    defaults[pair.Key] = value;
                }
            }

            return defaults;
        }

        public static GriddlyContext GetOrCreateGriddlyContext(this ControllerBase controller)
        {
            var context = controller.ViewData[_contextKey] as GriddlyContext;

            if (context == null)
            {
                context = new GriddlyContext()
                {
                    Name = (controller.GetType().Name + "_" + controller.ControllerContext.RouteData.GetRequiredString("action")).ToLower()
                };

                // NOTE: for 2020 Chris... yes, this is unique for multiple griddlies on a page as it is in the grid action context of each one
                controller.ViewData[_contextKey] = context;
            }

            return context;
        }

        static IDictionary<string, object> ObjectToDictionary(object value)
        {
            if (value == null)
                return null;

            return value.GetType()
                        .GetProperties()
                        .ToDictionary(p => p.Name, p => p.GetValue(value, null));
        }

        public static string Current(this UrlHelper helper, object routeValues = null, bool includeQueryString = false)
        {
            RouteValueDictionary values = new RouteValueDictionary();
            StringBuilder arrayVals = new StringBuilder();
            foreach (KeyValuePair<string, object> value in helper.RequestContext.RouteData.Values)
            {
                if (value.Value != null)
                {
                    Type t = value.Value.GetType();

                    if (t.IsPrimitive || t.IsEnum || t == typeof(Decimal) || t == typeof(String) || t == typeof(DateTime) || t == typeof(TimeSpan) || t == typeof(DateTimeOffset))
                        values[value.Key] = value.Value;
                    else if (t.HasCastOperator<DateTime>())
                        // values[value.Key] = (DateTime)value.Value; -- BAD: can't unbox a value type as a different type
                        values[value.Key] = Convert.ChangeType(value.Value, typeof(DateTime));
                    else if (t.IsArray || t.IsSubclassOf(typeof(IEnumerable)))
                    {
                        arrayVals.Append(string.Join("&", ((IEnumerable)value.Value).Cast<object>().Select(x=> value.Key + "=" + x.ToString())));
                    }
                }
            }

            if (includeQueryString)
            {
                foreach (string key in helper.RequestContext.HttpContext.Request.QueryString)
                    values[key] = helper.RequestContext.HttpContext.Request.QueryString[key];
            }

            if (routeValues != null)
            {
                foreach (KeyValuePair<string, object> value in ObjectToDictionary(routeValues))
                {
                    if (value.Value != null)
                    {
                        Type t = value.Value.GetType();

                        if (t.IsPrimitive || t.IsEnum || t == typeof(Decimal) || t == typeof(String) || t == typeof(DateTime) || t == typeof(TimeSpan) || t == typeof(DateTimeOffset))
                            values[value.Key] = value.Value;
                        else if (t.HasCastOperator<DateTime>())
                            // values[value.Key] = (DateTime)value.Value; -- BAD: can't unbox a value type as a different type
                            values[value.Key] = Convert.ChangeType(value.Value, typeof(DateTime));
                    }
                }
            }

            var route = helper.RouteUrl(values);
            if(arrayVals.Length>0)
            {
                route += (route.Contains("?") ? "&" : "?") + arrayVals.ToString();
            }
            return route;
        }

        static readonly PropertyInfo _instrumentationService = typeof(WebPageExecutingBase).GetProperty("InstrumentationService", BindingFlags.NonPublic | BindingFlags.Instance);
        static readonly PropertyInfo _isAvailableProperty = typeof(InstrumentationService).GetProperty("IsAvailable");

        public static void DisableInstrumentation(this WebPageExecutingBase page)
        {
            _isAvailableProperty.SetValue(_instrumentationService.GetValue(page), false);
        }
    }
}
