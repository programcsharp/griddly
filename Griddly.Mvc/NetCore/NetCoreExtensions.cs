#if NETCOREAPP

using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Linq.Expressions;
using System;

namespace Griddly.Mvc
{
    public enum SortDirection
    {
        Ascending,
        Descending
    }

    internal static class NetCoreExtensions
    {
        internal static string ToHtmlString(this HtmlString value)
        {
            return value.Value;
        }
        internal static NameValueCollection GetParams(this HttpRequest request)
        {
            var dict = request.Query.ToDictionary(x => x.Key, x => x.Value);

            if (request.Method == "POST")
            {
                foreach (var item in request.Form)
                {
                    if (dict.ContainsKey(item.Key))
                        dict[item.Key] = new StringValues(dict[item.Key].Union(item.Value).ToArray());
                    else
                        dict[item.Key] = item.Value;
                }
            }

            var result = new NameValueCollection();
            foreach (var item in dict)
                result.Add(item.Key, string.Join(",", item.Value));
            return result;
        }
    }

    internal static class ExpressionHelper
    {
        public static string GetExpressionText<TRow, TProp>(Expression<Func<TRow, TProp>> expression, IHtmlHelper html)
            => GetExpressionText(expression, html, out _);

        public static string GetExpressionText<TRow, TProp>(Expression<Func<TRow, TProp>> expression, IHtmlHelper html, out ModelMetadata metadata)
            => GetExpressionText(expression, html.ViewContext.HttpContext, out metadata);

        public static string GetExpressionText<TRow, TProp>(Expression<Func<TRow, TProp>> expression, HttpContext context)
            => GetExpressionText(expression, context, out _);

        public static string GetExpressionText<TRow, TProp>(Expression<Func<TRow, TProp>> expression, HttpContext context, out ModelMetadata metadata)
        {
            var expressionProvider = context.RequestServices.GetService<ModelExpressionProvider>();
            var metadataProvider = context.RequestServices.GetService<IModelMetadataProvider>();

            var modelExpression = expressionProvider.CreateModelExpression(new ViewDataDictionary<TRow>(metadataProvider, new ModelStateDictionary()), expression);
            metadata = modelExpression.Metadata;
            return modelExpression.Name;
        }
    }
}

namespace Griddly.Mvc.InternalExtensions
{
    public static class NetCoreInternalExtensions
    {
        public static string ToHtmlString(this IHtmlContent value)
        {
            using (var tw = new StringWriter())
            {
                value.WriteTo(tw, HtmlEncoder.Default);
                return tw.ToString();
            }
        }
        public static bool IsChildAction(this HttpContext context)
        {
            return context.Items["IsChildAction"] != null && (bool)context.Items["IsChildAction"];
        }
        public static ViewContext ParentActionViewContext(this HttpContext context)
        {
            return context.Items["IsChildAction"] != null && (bool)context.Items["IsChildAction"] ? (context.Items["ParentActionViewContext"] as ViewContext) : null;
        }
    }
}

#endif