#if NETCOREAPP
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Griddly.Mvc
{
    //https://stackoverflow.com/questions/26916664/html-action-in-asp-net-core
    public static class HtmlHelperViewExtensions
    {
        public static async Task<IHtmlContent> RenderAction(this IHtmlHelper helper, string action, string controller = null, object parameters = null)
        {
            if (controller == null)
                controller = (string)helper.ViewContext.RouteData.Values["controller"];

            var area = (string)helper.ViewContext.RouteData.Values["area"];
            return await RenderAction(helper, action, controller, area, parameters);
        }

        public static async Task<IHtmlContent> RenderAction(this IHtmlHelper helper, string action, string controller, string area, object parameters = null)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));
            if (controller == null)
                throw new ArgumentNullException(nameof(controller));

            return await RenderActionAsync(helper, action, controller, area, parameters);
        }

        private static async Task<IHtmlContent> RenderActionAsync(this IHtmlHelper helper, string action, string controller, string area, object parameters = null)
        {
            // fetching required services for invocation
            var serviceProvider = helper.ViewContext.HttpContext.RequestServices;
            var actionContextAccessor = helper.ViewContext.HttpContext.RequestServices.GetRequiredService<IActionContextAccessor>();
            var httpContextAccessor = helper.ViewContext.HttpContext.RequestServices.GetRequiredService<IHttpContextAccessor>();
            var actionSelector = serviceProvider.GetRequiredService<IActionSelector>();

            // creating new action invocation context
            var routeData = new RouteData();
            foreach (var router in helper.ViewContext.RouteData.Routers)
            {
                routeData.PushState(router, null, null);
            }
            routeData.PushState(null, new RouteValueDictionary(new { controller = controller, action = action, area = area }), null);
            routeData.PushState(null, new RouteValueDictionary(parameters ?? new { }), null);

            //get the actiondescriptor
            RouteContext routeContext = new RouteContext(helper.ViewContext.HttpContext) { RouteData = routeData };
            var candidates = actionSelector.SelectCandidates(routeContext);
            var actionDescriptor = actionSelector.SelectBestCandidate(routeContext, candidates);

            var originalActionContext = actionContextAccessor.ActionContext;
            var originalhttpContext = httpContextAccessor.HttpContext;
            try
            {
                var features = new FeatureCollection(helper.ViewContext.HttpContext.Features);
                features.Set<IItemsFeature>(new ItemsFeature()); //the child request should have it's own collection of Items
                var newHttpContext = serviceProvider.GetRequiredService<IHttpContextFactory>().Create(features);
                newHttpContext.Items["IsChildAction"] = true;
                newHttpContext.Items["ParentActionViewContext"] = helper.ViewContext;

                if (newHttpContext.Items.ContainsKey(typeof(IUrlHelper)))
                {
                    newHttpContext.Items.Remove(typeof(IUrlHelper));
                }
                newHttpContext.Response.Body = new MemoryStream();
                var actionContext = new ActionContext(newHttpContext, routeData, actionDescriptor);
                actionContextAccessor.ActionContext = actionContext;
                var invoker = serviceProvider.GetRequiredService<IActionInvokerFactory>().CreateInvoker(actionContext);
                await invoker.InvokeAsync();
                newHttpContext.Response.Body.Position = 0;
                using (var reader = new StreamReader(newHttpContext.Response.Body))
                {
                    return new HtmlString(reader.ReadToEnd());
                }
            }
            //catch (Exception ex)
            //{
            //    return new HtmlString(ex.Message);
            //}
            finally
            {
                actionContextAccessor.ActionContext = originalActionContext;
                httpContextAccessor.HttpContext = originalhttpContext;
                if (helper.ViewContext.HttpContext.Items.ContainsKey(typeof(IUrlHelper)))
                {
                    helper.ViewContext.HttpContext.Items.Remove(typeof(IUrlHelper));
                }
            }
        }
        //private static async Task<IHtmlContent> RenderActionAsync(this IHtmlHelper helper, string action, string controller, string area, object parameters = null)
        //{
        //    // fetching required services for invocation
        //    var currentHttpContext = helper.ViewContext.HttpContext;
        //    var httpContextFactory = currentHttpContext.RequestServices.GetRequiredService<IHttpContextFactory>();
        //    //var actionInvokerFactory = currentHttpContext.RequestServices.GetRequiredService<IActionInvokerFactory>();
        //    var actionSelector = currentHttpContext.RequestServices.GetRequiredService<IActionDescriptorCollectionProvider>();

        //    // creating new action invocation context
        //    var routeData = new RouteData();
        //    var routeParams = new RouteValueDictionary(parameters ?? new { });
        //    var routeValues = new RouteValueDictionary(new { area, controller, action });
        //    var features = new FeatureCollection(currentHttpContext.Features);
        //    features.Set<IItemsFeature>(new ItemsFeature()); //the child request should have it's own collection of Items
        //    var newHttpContext = httpContextFactory.Create(features);
        //    newHttpContext.Items["IsChildAction"] = true;
        //    newHttpContext.Items["ParentActionViewContext"] = helper.ViewContext;

        //    newHttpContext.Response.Body = new MemoryStream();

        //    foreach (var router in helper.ViewContext.RouteData.Routers)
        //        routeData.PushState(router, null, null);

        //    routeData.PushState(null, routeValues, null);
        //    routeData.PushState(null, routeParams, null);

        //    var actionDescriptor = actionSelector.ActionDescriptors.Items.First(i => i.RouteValues["Controller"] == controller && i.RouteValues["Action"] == action);
        //    var actionContext = new ActionContext(newHttpContext, routeData, actionDescriptor);
        //    var actionInvokerFactory = newHttpContext.RequestServices.GetService<IActionInvokerFactory>();

        //    // invoke action and retreive the response body
        //    var invoker = actionInvokerFactory.CreateInvoker(actionContext);
        //    string content = null;

        //    await invoker.InvokeAsync().ContinueWith(task =>
        //    {
        //        if (task.IsFaulted)
        //        {
        //            content = task.Exception.Message + Environment.NewLine + Environment.NewLine + task.Exception.StackTrace;
        //        }
        //        else if (task.IsCompleted)
        //        {
        //            newHttpContext.Response.Body.Position = 0;
        //            using (var reader = new StreamReader(newHttpContext.Response.Body))
        //                content = reader.ReadToEnd();
        //        }
        //    });

        //    return new HtmlString(content);
        //}
    }
}
#endif