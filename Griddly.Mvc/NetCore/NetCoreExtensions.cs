using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System.Text.Encodings.Web;

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

            if (request.HasFormContentType)
            {
                foreach (var item in request.Form)
                {
                    dict[item.Key] = item.Value;
                }
            }

            var result = new NameValueCollection();
            foreach (var kv in dict)
                foreach (string val in kv.Value)
                    result.Add(kv.Key, val);
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

        public static IGriddlyConfig GetGriddlyConfig(this HttpContext ctx)
        {
            return ctx.RequestServices.GetRequiredService<IGriddlyConfig>();
        }
        public static IGriddlyConfig GetGriddlyConfig(this IHtmlHelper html)
        {
            return html.ViewContext.GetGriddlyConfig();
        }
        public static IGriddlyConfig GetGriddlyConfig(this ViewContext ctx)
        {
            return ctx.HttpContext.GetGriddlyConfig();
        }
        public static IGriddlyConfig GetGriddlyConfig(this ActionContext ctx)
        {
            return ctx.HttpContext.GetGriddlyConfig();
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