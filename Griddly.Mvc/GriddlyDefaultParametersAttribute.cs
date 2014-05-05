using System;
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
                if (filterContext.ActionDescriptor.GetFilterAttributes(true).Any(x => x.GetType() == typeof(GriddlyDefaultParametersAttribute))
                    || typeof(GriddlyResult).IsAssignableFrom(filterContext.GetExpectedReturnType()))
                {
                    GriddlySettings settings = GriddlySettingsResult.GetSettings(filterContext.Controller.ControllerContext, ViewName);

                    if (settings != null)
                    {
                        foreach (GriddlyFilter filter in settings.Filters.Union(settings.Columns.Where(x => x.Filter != null).Select(x => x.Filter)))
                        {
                            if (filter.Default != null)
                            {
                                object value = filter.Default;

                                GriddlyFilterList filterList = filter as GriddlyFilterList;

                                if (filterList != null && filterList.IsMultiple && !value.GetType().IsArray)
                                {
                                    Type type = filter.Default.GetType();

                                    if (filterList.IsNullable)
                                        type = typeof(Nullable<>).MakeGenericType(type);

                                    Array array = Array.CreateInstance(type, 1);

                                    array.SetValue(filter.Default, 0);

                                    value = array;
                                }

                                settings.FilterDefaults[filter.Field] = value;
                            }

                            GriddlyFilterRange filterRange = filter as GriddlyFilterRange;

                            if (filterRange != null && filterRange.DefaultEnd != null)
                                settings.FilterDefaults[filterRange.FieldEnd] = filterRange.DefaultEnd;
                        }

                        foreach (KeyValuePair<string, object> param in settings.FilterDefaults)
                        {
                            filterContext.ActionParameters[param.Key] = param.Value;
                            // TODO: use a value provider to make this work
                            //filterContext.RouteData.Values[param.Key] = param.Value;
                        }
                    }
                }
            }

            base.OnActionExecuting(filterContext);
        }
    }
}
