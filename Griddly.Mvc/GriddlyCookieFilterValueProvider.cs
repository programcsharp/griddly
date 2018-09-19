using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Griddly.Mvc
{
    public class GriddlyCookieFilterValueProvider : IValueProvider
    {
        GriddlyContext _context;

        public GriddlyCookieFilterValueProvider(GriddlyContext context)
        {
            _context = context;

            if (_context.CookieData?.SortFields?.Length > 0)
                _context.SortFields = _context.CookieData?.SortFields;
        }

        public bool ContainsPrefix(string prefix)
        {
            return _context.CookieData.Values?.ContainsKey(prefix) == true;
        }

        public ValueProviderResult GetValue(string key)
        {
            string[] value = null;

            if (_context.CookieData.Values?.TryGetValue(key, out value) != true)
                value = null;

            string attemptedValue = null;

            if (value != null)
                attemptedValue = string.Join(",", value);

            return new ValueProviderResult(value, attemptedValue, CultureInfo.CurrentCulture);
        }
    }

    public class GriddlyCookieFilterValueProviderFactory : ValueProviderFactory
    {
        public override IValueProvider GetValueProvider(ControllerContext controllerContext)
        {
            if (controllerContext.IsChildAction && controllerContext.HttpContext.Request.QueryString.Count == 0)
            {
                var context = controllerContext.Controller.GetOrCreateGriddlyContext();
                var cookie = controllerContext.HttpContext.Request.Cookies[context.CookieName];

                if (cookie != null && !string.IsNullOrWhiteSpace(cookie.Value))
                {
                    try
                    {
                        var data = JsonConvert.DeserializeObject<GriddlyFilterCookieData>(cookie.Value);

                        context.CookieData = data;
                        context.IsDefaultSkipped = true;

                        return new GriddlyCookieFilterValueProvider(context);
                    }
                    catch
                    {
                        // TODO: log it?
                    }
                }
            }

            return null;
        }
    }
}
