using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
                    typeof(GriddlyResult).IsAssignableFrom(GetExpectedReturnType(filterContext)))
                {
                    GriddlySettings settings = GriddlySettingsResult.GetSettings(filterContext.Controller.ControllerContext, ViewName);

                    foreach (KeyValuePair<string, object> param in settings.FilterDefaults)
                        filterContext.ActionParameters[param.Key] = param.Value;
                }
            }

            base.OnActionExecuting(filterContext);
        }

        Type GetExpectedReturnType(ActionExecutingContext filterContext)
        {
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
    }
}
