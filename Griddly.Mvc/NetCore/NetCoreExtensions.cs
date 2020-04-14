#if !NET45

using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Primitives;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;

namespace Griddly.Mvc
{
    public enum SortDirection
    {
        Ascending,
        Descending
    }

    public static class NetCoreExtensions
    {
        public static string ToHtmlString(this HelperResult value)
        {
            var sb = new StringBuilder();
            var tw = new StringWriter();
            value.WriteTo(tw, HtmlEncoder.Default);
            return sb.ToString();
        }
        public static string ToHtmlString(this HtmlString value)
        {
            return HtmlEncoder.Default.Encode(value.ToString());
        }
        public static NameValueCollection GetParams(this HttpRequest request)
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
        public static bool IsChildAction(this HttpContext context)
        {
            return context.Items["IsChildAction"] != null && (bool)context.Items["IsChildAction"];
        }
    }
}

#endif