using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Griddly.Mvc
{
    internal static class Extensions
    {
        // http://stackoverflow.com/a/2224421/8037
        // TODO: cache result in dictionary
        internal static bool HasCastOperator<T>(this Type type)
        {
            bool castable = type.GetMethods(BindingFlags.Public | BindingFlags.Static)
                            .Any(
                                m => m.ReturnType == typeof(T) &&
                                m.Name == "op_Implicit" ||
                                m.Name == "op_Explicit"
                            );
            return castable;
        }

        internal static Type GetExpectedReturnType(this ActionExecutingContext filterContext)
        {
            if (filterContext.ActionDescriptor is ReflectedActionDescriptor)
                return ((ReflectedActionDescriptor)filterContext.ActionDescriptor).MethodInfo.ReturnType;

            // Find out what type is expected to be returned
            string actionName = filterContext.ActionDescriptor.ActionName;
            Type controllerType = filterContext.Controller.GetType();
            MethodInfo actionMethodInfo = default(MethodInfo);
            try
            {
                actionMethodInfo = controllerType.GetMethod(actionName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            }
            catch (AmbiguousMatchException)
            {
                // Try to find a match using the parameters passed through
                var actionParams = filterContext.ActionParameters;
                List<Type> paramTypes = new List<Type>();

                foreach (var p in actionParams)
                    paramTypes.Add(p.Value.GetType());

                actionMethodInfo = controllerType.GetMethod(actionName, paramTypes.ToArray());
            }

            if (actionMethodInfo != null)
                return actionMethodInfo.ReturnType;
            else
                return null;
        }

        static readonly Regex _urlRegex = new Regex(@"(http|ftp|https):\/\/[\w\-_]+(\.[\w\-_]+)+([\w\-\.,@?^=%&amp;:/~\+#]*[\w\-\@?^=%&amp;/~\+#])?", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        internal static HtmlString GetHtmlView(string value, bool turnUrlsIntoLinks = false)
        {
            if (value == null)
                return null;

            value = HttpUtility.HtmlEncode(value).Replace("  ", "&nbsp; ")
                    .Replace("\r\n", "<br/>").Replace("\r", "<br/>").Replace("\n", "<br/>");

            if (turnUrlsIntoLinks)
            {
                foreach (Match match in _urlRegex.Matches(value))
                {
                    if (match != null && !string.IsNullOrWhiteSpace(match.Value))
                        value = value.Replace(match.Value, string.Format("<a href=\"{0}\">{1}</a>", HttpUtility.HtmlAttributeEncode(match.Value), match.Value));
                }
            }

            return new HtmlString(value);
        }

        internal static string ToStringDescription(Enum value)
        {
            if (value == null)
                return null;

            return GetEnumDescription(value.GetType().GetField(value.ToString()));
        }

        internal static IEnumerable<SelectListItem> ToSelectListItems<T>()
            where T : struct
        {
            Type enumType = typeof(T);

            if (!enumType.IsEnum)
                throw new InvalidOperationException("T must be an Enum.");

            foreach (FieldInfo field in enumType.GetFields(BindingFlags.Static | BindingFlags.GetField | BindingFlags.Public))
            {
                EditorBrowsableAttribute editorBrowsableAttribute = field.GetCustomAttribute<EditorBrowsableAttribute>();

                if (editorBrowsableAttribute == null || editorBrowsableAttribute.State != EditorBrowsableState.Never)
                {
                    yield return new SelectListItem()
                    {
                        Value = field.GetValue(null).ToString(),
                        Text = GetEnumDescription(field)
                    };
                }
            }
        }

        static string GetEnumDescription(FieldInfo fi)
        {
            DescriptionAttribute descriptionAttribute = fi.GetCustomAttribute<DescriptionAttribute>();

            if (descriptionAttribute != null)
                return descriptionAttribute.Description;

            DisplayAttribute displayAttribute = fi.GetCustomAttribute<DisplayAttribute>();

            if (displayAttribute != null)
                return displayAttribute.Name;

            return fi.Name;
        }

        internal static string GetFormattedValue(object value, FilterDataType dataType)
        {
            if (value == null)
                return null;
            else if (dataType == FilterDataType.String)
                return value.ToString();

            string format;

            switch (dataType)
            {
                case FilterDataType.Integer:
                    format = "d";

                    break;
                case FilterDataType.Decimal:
                    format = "n2";

                    break;
                case FilterDataType.Currency:
                    format = "c2";

                    break;
                //case FilterDataType.Percent:
                //    format = "p";

                //    break;
                case FilterDataType.Date:
                    format = "d";

                    break;
                default:
                    throw new InvalidOperationException("Invalid FilterDataType " + dataType);
            }

            string output = string.Format("{0:" + format + "}", value);

            if ((dataType == FilterDataType.Decimal || dataType == FilterDataType.Currency) && (output.EndsWith(".00") || output.EndsWith(",00")))
                output = output.Substring(0, output.Length - 3);

            return output;
        }

    }
}
