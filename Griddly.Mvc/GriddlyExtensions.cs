using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Web.Routing;

namespace Griddly.Mvc
{
    public static class GriddlyExtensions
    {
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
            return htmlHelper.Griddly(new GriddlyResultPage<T>(data), settings, true);
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

            foreach (KeyValuePair<string, object> value in helper.RequestContext.RouteData.Values)
            {
                if (value.Value != null)
                {
                    Type t = value.Value.GetType();

                    if (t.IsPrimitive || t == typeof(Decimal) || t == typeof(String) || t == typeof(DateTime) || t == typeof(TimeSpan) || t == typeof(DateTimeOffset))
                        values[value.Key] = value.Value;
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

                        if (t.IsPrimitive || t == typeof(Decimal) || t == typeof(String) || t == typeof(DateTime) || t == typeof(TimeSpan) || t == typeof(DateTimeOffset))
                            values[value.Key] = value.Value;
                    }
                }
            }

            return helper.RouteUrl(values);
        }
    }
}
