using System.Text;
using System.Threading;
#if NETFRAMEWORK
using Griddly.Mvc.Linq.Dynamic;
using System.Web.Mvc.Html;
using System.Web.WebPages;
using System.Web.WebPages.Instrumentation;
#else
using Microsoft.AspNetCore.Mvc.Razor;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Griddly.Mvc.InternalExtensions;
#endif

namespace Griddly.Mvc;

public static class GriddlyExtensions
{
    public static string CurrencySymbol
    {
        get
        {
            return Thread.CurrentThread.CurrentCulture.NumberFormat.CurrencySymbol;
        }
    }

#if NETFRAMEWORK
    public static MvcHtmlString Griddly(this HtmlHelper htmlHelper, string actionName)
    {
        return htmlHelper.Griddly(actionName, null);
    }
#else
    public static async Task<IHtmlContent> GriddlyAsync(this IHtmlHelper htmlHelper, string actionName)
    {
        return await htmlHelper.GriddlyAsync(actionName, null);
    }
#endif

#if NETFRAMEWORK
    public static MvcHtmlString Griddly(this HtmlHelper htmlHelper, string actionName, object routeValues)
    {
        return htmlHelper.Griddly(actionName, null, routeValues);
    }
#else
    public static async Task<IHtmlContent> GriddlyAsync(this IHtmlHelper htmlHelper, string actionName, object routeValues)
    {
        return await htmlHelper.GriddlyAsync(actionName, null, routeValues);
    }
#endif

#if NETFRAMEWORK
    public static MvcHtmlString Griddly(this HtmlHelper htmlHelper, string actionName, string controllerName)
    {
        return htmlHelper.Griddly(actionName, controllerName, null);
    }
#else
    public static async Task<IHtmlContent> GriddlyAsync(this IHtmlHelper htmlHelper, string actionName, string controllerName)
    {
        return await htmlHelper.GriddlyAsync(actionName, controllerName, null);
    }
#endif

#if NETFRAMEWORK
    public static MvcHtmlString Griddly(this HtmlHelper htmlHelper, string actionName, string controllerName, object routeValues)
    {
        // TODO: validate that we got a GriddlyResult
        return htmlHelper.Action(actionName, controllerName, routeValues);
    }
#else
    public static async Task<IHtmlContent> GriddlyAsync(this IHtmlHelper htmlHelper, string actionName, string controllerName, object routeValues)
    {
        // TODO: validate that we got a GriddlyResult
        return await htmlHelper.RenderAction(actionName, controllerName, routeValues);
    }
#endif

#if NETFRAMEWORK
    public static MvcHtmlString Griddly(this HtmlHelper htmlHelper, GriddlySettings settings)
    {
        return htmlHelper.Griddly((GriddlyResultPage)htmlHelper.ViewData.Model, settings);
    }
#else
    public static async Task<IHtmlContent> GriddlyAsync(this IHtmlHelper htmlHelper, GriddlySettings settings)
    {
        return await htmlHelper.GriddlyAsync((GriddlyResultPage)htmlHelper.ViewData.Model, settings);
    }
#endif

#if NETFRAMEWORK
    public static MvcHtmlString SimpleGriddly<T>(this HtmlHelper htmlHelper, GriddlySettings<T> settings, IEnumerable<T> data)
#else
    public static async Task<IHtmlContent> SimpleGriddlyAsync<T>(this IHtmlHelper htmlHelper, GriddlySettings<T> settings, IEnumerable<T> data)
#endif
    {
        // TODO: figure out how to get this in one query
        foreach (GriddlyColumn c in settings.Columns.Where(x => x.SummaryFunction != null))
            PopulateSummaryValue(data, c);

#if NETFRAMEWORK
        return htmlHelper.Griddly(new GriddlyResultPage<T>(data), settings, true);
#else
        return await htmlHelper.GriddlyAsync(new GriddlyResultPage<T>(data), settings, true);
#endif
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
                try
                {
                    IQueryable q = data.AsQueryable();
                    if (c.ExpressionString.Contains("."))
                        q = q.Select(c.ExpressionString.Substring(0, c.ExpressionString.LastIndexOf('.')));

                    c.SummaryValue = q.Aggregate(c.SummaryFunction.Value.ToString(), c.ExpressionString.Split(new[] { '.' }).Last());
                }
                catch (Exception ex) when (ex.InnerException is ArgumentNullException)
                {
                    c.SummaryValue = null;
                }
                catch (InvalidOperationException ex) when (ex.Message.Contains("failed because the materialized value is null") || ex.Message.Contains("Nullable object must have a value"))
                {
                    c.SummaryValue = null;
                }
                break;

            default:
                throw new InvalidOperationException(string.Format("Unknown summary function {0} for column {1}.", c.SummaryFunction, c.ExpressionString));
        }
    }

#if NETFRAMEWORK
    public static MvcHtmlString Griddly(this HtmlHelper htmlHelper, GriddlyResultPage model, GriddlySettings settings, bool isSimpleGriddly = false)
#else
    public static async Task<IHtmlContent> GriddlyAsync(this IHtmlHelper htmlHelper, GriddlyResultPage model, GriddlySettings settings, bool isSimpleGriddly = false)
#endif
    {
        if (htmlHelper.ViewData["_isGriddlySettingsRequest"] as bool? != true)
        {
#if NETFRAMEWORK
            ViewDataDictionary viewData = new ViewDataDictionary(htmlHelper.ViewData);
            viewData.Model = model;
#else
            ViewDataDictionary viewData = new ViewDataDictionary<GriddlyResultPage>(htmlHelper.ViewData, model);
#endif

            viewData["settings"] = settings;
            viewData["isSimpleGriddly"] = isSimpleGriddly;

#if NETFRAMEWORK
            return htmlHelper.Partial("~/Views/Shared/Griddly/Griddly.cshtml", viewData);
#else
            return await htmlHelper.PartialAsync("~/Pages/Shared/Griddly/Griddly.cshtml", model, viewData);
#endif
        }
        else
        {
            htmlHelper.ViewContext.ViewData["settings"] = settings;

            return null;
        }
    }

#if NETFRAMEWORK
    public static MvcHtmlString GriddlyFilterBar(this HtmlHelper htmlHelper, GriddlyFilterBarSettings settings)
    {
        return htmlHelper.Partial("~/Views/Shared/Griddly/GriddlyFilterBar.cshtml", settings);
    }
#else
    public static async Task<IHtmlContent> GriddlyFilterBarAsync(this IHtmlHelper htmlHelper, GriddlyFilterBarSettings settings)
    {
        return await htmlHelper.PartialAsync("~/Pages/Shared/Griddly/GriddlyFilterBar.cshtml", settings, null);
    }
#endif

#if NETFRAMEWORK
    public static GriddlyColumn<TRow> GriddlyColumnFor<TRow>(this HtmlHelper<IEnumerable<TRow>> htmlHelper, Func<TRow, object> template)
#else
    public static GriddlyColumn<TRow> GriddlyColumnFor<TRow>(this IHtmlHelper<IEnumerable<TRow>> htmlHelper, Func<TRow, object> template)
#endif
    {
        return htmlHelper.GriddlyColumnFor<TRow>(template, null);
    }

#if NETFRAMEWORK
    public static GriddlyColumn<TRow> GriddlyColumnFor<TRow>(this HtmlHelper<IEnumerable<TRow>> htmlHelper, Func<TRow, object> template, string caption, string columnId = null)
#else
    public static GriddlyColumn<TRow> GriddlyColumnFor<TRow>(this IHtmlHelper<IEnumerable<TRow>> htmlHelper, Func<TRow, object> template, string caption, string columnId = null)
#endif
    {
        return new GriddlyColumn<TRow>(null, caption, columnId)
        {
            Template = template
        };
    }

#if NETFRAMEWORK
    public static HtmlString AttributeNullable(this HtmlHelper helper, string name, string value)
#else
    public static HtmlString AttributeNullable(this IHtmlHelper helper, string name, string value)
#endif
    {
        if (value == null)
            return null;
        else
            return new HtmlString(name + "=\"" + helper.Encode(value) + "\"");
    }

#if NETFRAMEWORK
    public static HtmlString AttributeIf(this HtmlHelper helper, string name, bool shouldShow, Func<object, object> value)
#else
    public static HtmlString AttributeIf(this IHtmlHelper helper, string name, bool shouldShow, Func<object, object> value)
#endif
    {
        if (shouldShow)
            return helper.AttributeIf(name, shouldShow, value(null));
        else
            return null;
    }

#if NETFRAMEWORK
    public static HtmlString AttributeIf(this HtmlHelper helper, string name, bool shouldShow, object value)
#else
    public static HtmlString AttributeIf(this IHtmlHelper helper, string name, bool shouldShow, object value)
#endif
    {
        if (shouldShow)
            return new HtmlString(name + "=\"" + value + "\"");
        else
            return null;
    }

    // http://stackoverflow.com/a/18618808/8037
    public static HtmlString ToHtmlAttributes(this IDictionary<string, object> dictionary)
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

#if NETFRAMEWORK
    public static void SetGriddlyDefault<T>(this ControllerBase controller, ref T parameter, string field, T value, bool? ignoreSkipped = null)
#else
    public static void SetGriddlyDefault<T>(this Controller controller, ref T parameter, string field, T value, bool? ignoreSkipped = null)
#endif
    {
        if (ignoreSkipped == null)
            ignoreSkipped = GriddlyParameterAttribute.DefaultIgnoreSkipped;

        var context = controller.GetOrCreateGriddlyContext();

        context.Defaults[field] = value;

#if NETFRAMEWORK
        if (controller.ControllerContext.IsChildAction
#else
        if (controller.HttpContext.IsChildAction()
#endif
            && (!context.IsDefaultSkipped || ignoreSkipped.Value)
            && (EqualityComparer<T>.Default.Equals(parameter, default(T))
#if NETCOREAPP
            || typeof(T).IsArray && (parameter as Array)?.Length == 0 //In .NET core, the default value for array is null, but MVC binds the parameter as zero length array
#endif
            ))
        {
            parameter = value;

            context.Parameters[field] = parameter;
        }
    }

#if NETFRAMEWORK
    public static void SetGriddlyDefault<T>(this ControllerBase controller, ref T[] parameter, string field, IEnumerable<T> value, bool? ignoreSkipped = null)
#else
    public static void SetGriddlyDefault<T>(this Controller controller, ref T[] parameter, string field, IEnumerable<T> value, bool? ignoreSkipped = null)
#endif
    {
        if (ignoreSkipped == null)
            ignoreSkipped = GriddlyParameterAttribute.DefaultIgnoreSkipped;

        var context = controller.GetOrCreateGriddlyContext();

        context.Defaults[field] = value;

#if NETFRAMEWORK
        if (controller.ControllerContext.IsChildAction
#else
        if (controller.HttpContext.IsChildAction()
#endif
            && (!context.IsDefaultSkipped || ignoreSkipped.Value)
            && parameter == null)
        {
            parameter = value.ToArray();

            context.Parameters[field] = parameter;
        }
    }

#if NETFRAMEWORK
    public static void SetGriddlyDefault<T>(this ControllerBase controller, ref T?[] parameter, string field, IEnumerable<T> value, bool? ignoreSkipped = null)
#else
    public static void SetGriddlyDefault<T>(this Controller controller, ref T?[] parameter, string field, IEnumerable<T> value, bool? ignoreSkipped = null)
#endif
        where T : struct
    {
        if (ignoreSkipped == null)
            ignoreSkipped = GriddlyParameterAttribute.DefaultIgnoreSkipped;

        var context = controller.GetOrCreateGriddlyContext();

        context.Defaults[field] = value;

#if NETFRAMEWORK
        if (controller.ControllerContext.IsChildAction
#else
        if (controller.HttpContext.IsChildAction()
#endif
            && (!context.IsDefaultSkipped || ignoreSkipped.Value)
            && parameter == null)
        {
            parameter = value.Cast<T?>().ToArray();

            context.Parameters[field] = parameter;
        }
    }

    public static void SetGriddlyDefault<TController, TModel, TProp>(this TController controller, TModel model,
        Expression<Func<TModel, TProp>> expression, TProp defaultValue, bool? ignoreSkipped = null)
        where TController : Controller
    {
        if (ignoreSkipped == null)
            ignoreSkipped = GriddlyParameterAttribute.DefaultIgnoreSkipped;

        var context = controller.GetOrCreateGriddlyContext();

#if NETFRAMEWORK
        var field = ExpressionHelper.GetExpressionText(expression);
#else
        var field = ExpressionHelper.GetExpressionText(expression, controller.HttpContext);
#endif
        context.Defaults[field] = defaultValue;

        var compiledExpression = expression.Compile();
        TProp parameter = compiledExpression(model);

#if NETFRAMEWORK
        if (controller.ControllerContext.IsChildAction
#else
        if (controller.HttpContext.IsChildAction()
#endif
            && (!context.IsDefaultSkipped || ignoreSkipped.Value)
            && EqualityComparer<TProp>.Default.Equals(parameter, default(TProp)))
        {
            parameter = defaultValue;

            context.Parameters[field] = defaultValue;

            if (expression.Body is MemberExpression me && me.Member.MemberType == MemberTypes.Property)
            {
                var pi = me.Member as PropertyInfo;
                pi.SetValue(model, defaultValue);
            }
            else
            {
                throw new ArgumentException("expression must be a MemberExpression to a Property");
            }
        }
    }

#if NETFRAMEWORK
    public static object GetGriddlyDefault(this WebViewPage page, string field)
#else
    public static object GetGriddlyDefault(this RazorPageBase page, string field)
#endif
    {
        object value = null;

        if ((page.ViewContext.ViewData[_contextKey] as GriddlyContext)?.Defaults.TryGetValue(field, out value) != true)
            value = null;

        return value;
    }

#if NETFRAMEWORK
    public static object GetGriddlyParameter(this WebViewPage page, string field)
#else
    public static object GetGriddlyParameter(this RazorPageBase page, string field)
#endif
    {
        object value = null;

        if ((page.ViewContext.ViewData[_contextKey] as GriddlyContext)?.Parameters.TryGetValue(field, out value) != true)
            value = null;

        return value;
    }

#if NETFRAMEWORK
    public static int GetGriddlyParameterCount(this WebViewPage page)
#else
    public static int GetGriddlyParameterCount(this RazorPageBase page)
#endif
    {
        return (page.ViewContext.ViewData[_contextKey] as GriddlyContext)?.Parameters.Count ?? 0;
    }

#if NETFRAMEWORK
    public static Dictionary<string, object> GetGriddlyDefaults(this WebViewPage page)
#else
    public static Dictionary<string, object> GetGriddlyDefaults(this RazorPageBase page)
#endif
    {
        Dictionary<string, object> defaults = new Dictionary<string, object>();
        var context = page.ViewContext.ViewData[_contextKey] as GriddlyContext;

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

#if NETCOREAPP
    public static GriddlyContext GetOrCreateGriddlyContext(this ActionContext actionContext)
    {
        var context = GetOrCreateGriddlyContext(actionContext.RouteData, actionContext.HttpContext);
        if (actionContext is ViewContext vc)
            vc.ViewData[_contextKey] = context;
        return context;
    }

    public static GriddlyContext GetOrCreateGriddlyContext(this Controller controller)
    {
        return GetOrCreateGriddlyContext(controller.RouteData, controller.HttpContext);
    }
#endif

#if NETFRAMEWORK
    public static GriddlyContext GetOrCreateGriddlyContext(this ControllerBase controller)
    {
        var context = controller.ViewData[_contextKey] as GriddlyContext;
#else
    private static GriddlyContext GetOrCreateGriddlyContext(RouteData routeData, HttpContext httpContext)
    {
        var context = httpContext.Items[_contextKey] as GriddlyContext;
#endif

        if (context == null)
        {
            SortField[] sortFields = null;
            GriddlyExportFormat? exportFormat;

            NameValueCollection items;

#if NETFRAMEWORK
            if (controller.ControllerContext.HttpContext.Request.Params != null)
                items = new NameValueCollection(controller.ControllerContext.HttpContext.Request.Params);
#else
            if ((httpContext.Request.Method == "POST" && httpContext.Request.Form != null) || httpContext.Request.Query != null)
                items = new NameValueCollection(httpContext.Request.GetParams());
#endif
            else
                items = new NameValueCollection();

            if (!int.TryParse(items["pageNumber"], out int pageNumber))
                pageNumber = 0;

            if (!int.TryParse(items["pageSize"], out int pageSize))
                pageSize = 20;

            if (Enum.TryParse(items["exportFormat"], true, out GriddlyExportFormat exportFormatValue))
                exportFormat = exportFormatValue;
            else
                exportFormat = null;

            sortFields = GriddlyResult.GetSortFields(items);

            context = new GriddlyContext()
            {
#if NETFRAMEWORK
                Name = (controller.GetType().Name + "_" + controller.ControllerContext.RouteData.Values["action"] as string).ToLower(),
#else
                Name = (routeData.Values["controller"] as string).ToLower() + "_" + (routeData.Values["action"] as string).ToLower(),
#endif
                PageNumber = pageNumber,
                PageSize = pageSize,
                ExportFormat = exportFormat,
                SortFields = sortFields
            };

            // NOTE: for 2020 Chris... yes, this is unique for multiple griddlies on a page as it is in the grid action context of each one
#if NETFRAMEWORK
            controller.ViewData[_contextKey] = context;
#else
            httpContext.Items[_contextKey] = context;
#endif
        }

        return context;
    }

    // TODO: keep in sync with Extensions.GetFormattedValue
    public static string[] GetFormattedValueByType(object value)
    {
        if (value != null)
        {
            var type = value.GetType();

            if (value is IEnumerable enumerable && type != typeof(string))
                return enumerable.Cast<object>().Select(x => x?.ToString()).ToArray();

            string stringValue;

            if (type == typeof(float) ||
                type == typeof(double) ||
                type == typeof(decimal))
                stringValue = string.Format("{0:n2}", value);
            else if (type == typeof(DateTime) || type.HasCastOperator<DateTime>())
                stringValue = string.Format("{0:d}", value);
            else if (type == typeof(bool))
                stringValue = value.ToString().ToLower();
            else
                stringValue = value.ToString();

            if (!string.IsNullOrWhiteSpace(stringValue))
                return new[] { stringValue };
        }

        return null;
    }


    static IDictionary<string, object> ObjectToDictionary(object value)
    {
        if (value == null)
            return null;

        return value.GetType()
                    .GetProperties()
                    .ToDictionary(p => p.Name, p => p.GetValue(value, null));
    }

#if NETFRAMEWORK
    public static string Current(this UrlHelper helper, object routeValues = null, bool includeQueryString = false)
    {
        var actionRouteValues = helper.RequestContext.RouteData.Values;
#else
    public static string Current(this IUrlHelper helper, ViewContext vc, object routeValues = null, bool includeQueryString = false)
    {
        var actionRouteValues = vc.RouteData.Values;
#endif
        RouteValueDictionary values = new RouteValueDictionary();
        StringBuilder arrayVals = new StringBuilder();

        foreach (var value in actionRouteValues)
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
                    if (arrayVals.Length > 0)
                        arrayVals.Append("&");
                    arrayVals.Append(string.Join("&", ((IEnumerable)value.Value).Cast<object>().Select(x=> value.Key + "=" + x?.ToString())));
                }
            }
        }

        if (includeQueryString)
        {
#if NETFRAMEWORK
            foreach (string key in helper.RequestContext.HttpContext.Request.QueryString)
                values[key] = helper.RequestContext.HttpContext.Request.QueryString[key];
#else
            foreach (string key in helper.ActionContext.HttpContext.Request.Query.Keys)
                values[key] = helper.ActionContext.HttpContext.Request.Query[key];
#endif
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

#if NETFRAMEWORK
    static readonly PropertyInfo _instrumentationService = typeof(WebPageExecutingBase).GetProperty("InstrumentationService", BindingFlags.NonPublic | BindingFlags.Instance);
    static readonly PropertyInfo _isAvailableProperty = typeof(InstrumentationService).GetProperty("IsAvailable");

    public static void DisableInstrumentation(this WebPageExecutingBase page)
    {
        _isAvailableProperty.SetValue(_instrumentationService.GetValue(page), false);
    }
#endif
}
