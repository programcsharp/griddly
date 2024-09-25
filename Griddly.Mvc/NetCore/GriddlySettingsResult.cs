﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System.Threading.Tasks;

namespace Griddly.Mvc;

public class GriddlySettingsResult
{
    public static async Task<GriddlySettings> GetSettings(ActionContext context, string viewName, ViewDataDictionary viewData)
    {
        if (context == null)
            throw new ArgumentNullException("context");

        if (string.IsNullOrEmpty(viewName))
            viewName = context.RouteData.Values["action"].ToString();

        var sp = context.HttpContext.RequestServices;

        var httpContext = sp.GetRequiredService<IHttpContextFactory>().Create(context.HttpContext.Features);

        var actionContext = new ActionContext(httpContext, context.RouteData, context.ActionDescriptor);

        var viewEngine = sp.GetRequiredService<IRazorViewEngine>();
        var viewResult = viewEngine.FindView(context, viewName, false);

        //Workaround for https://github.com/dotnet/aspnetcore/issues/38373
        if (viewResult.View == null && viewName.StartsWith("~/"))
            viewResult = viewEngine.GetView("", viewName, true);

        if (viewResult.View == null)
        {
            throw new ArgumentNullException($"{viewName} does not match any available view");
        }

        using (var writer = new StringWriter())
        {
            var vdd = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary());

            vdd["_isGriddlySettingsRequest"] = true;

            if (viewData != null)
            {
                foreach (KeyValuePair<string, object> value in viewData)
                    vdd[value.Key] = value.Value;
            }

            var tdd = new TempDataDictionary(actionContext.HttpContext, sp.GetRequiredService<ITempDataProvider>());

            var viewContext = new ViewContext(actionContext, viewResult.View, vdd, tdd, writer, new HtmlHelperOptions());

            await viewResult.View.RenderAsync(viewContext);

            return viewContext.ViewData["settings"] as GriddlySettings;
        }
    }
}