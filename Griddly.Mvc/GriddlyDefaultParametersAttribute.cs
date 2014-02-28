using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Griddly.Mvc
{
    public class GriddlyDefaultParametersAttribute : ActionFilterAttribute
    {
        public string ViewName { get; protected set; }

        public GriddlyDefaultParametersAttribute()
        { }

        public GriddlyDefaultParametersAttribute(string viewName)
        {
            ViewName = viewName;
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext.IsChildAction)
            {
                if (filterContext.ActionDescriptor.GetFilterAttributes(true).Any(x => x.GetType() == typeof(GriddlyDefaultParametersAttribute)) ||
                    typeof(GriddlyResult).IsAssignableFrom(filterContext.GetExpectedReturnType()))
                {
                    GriddlySettings settings = GriddlySettingsResult.GetSettings(filterContext.Controller.ControllerContext, ViewName);

                    foreach (GriddlyFilter filter in settings.Filters.Union(settings.Columns.Where(x => x.Filter != null).Select(x => x.Filter)))
                    {
                        if (filter.Default != null)
                            settings.FilterDefaults[filter.Field] = filter.Default;

                        GriddlyFilterRange rangeFilter = filter as GriddlyFilterRange;

                        if (rangeFilter != null && rangeFilter.DefaultEnd != null)
                            settings.FilterDefaults[rangeFilter.FieldEnd] = rangeFilter.DefaultEnd;
                    }

                    foreach (KeyValuePair<string, object> param in settings.FilterDefaults)
                        filterContext.ActionParameters[param.Key] = param.Value;
                }
            }

            base.OnActionExecuting(filterContext);
        }
    }
}
