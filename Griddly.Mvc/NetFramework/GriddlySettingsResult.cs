#if NET45
using System;
using System.Collections;
using System.Collections.Specialized;
using System.IO;
using System.Web;
using System.Web.Instrumentation;
using System.Web.Mvc;
using System.Web.Routing;

namespace Griddly.Mvc
{
    public class GriddlySettingsResult : PartialViewResult
    {
        public GriddlySettings Settings { get; protected set; }

        public override void ExecuteResult(ControllerContext context)
        {
            if (context == null)
                throw new ArgumentNullException("context");

            if (string.IsNullOrEmpty(ViewName))
                this.ViewName = context.RouteData.GetRequiredString("action");

            ViewEngineResult result = null;

            if (View == null)
                View = FindView(context).View;

            using (StringWriter output = new StringWriter())
            {
                ViewData["_isGriddlySettingsRequest"] = true;

                ViewContext viewContext = new ViewContext(context, View, ViewData, TempData, output);

                View.Render(viewContext, output);

                Settings = ViewData["settings"] as GriddlySettings;
            }

            if (result != null)
                result.ViewEngine.ReleaseView(context, View);
        }

        public static GriddlySettings GetSettings(ControllerContext context, string viewName)
        {
            try
            {
                GriddlySettingsResult settingsResult = new GriddlySettingsResult()
                {
                    ViewName = viewName,
                    ViewData = context.Controller.ViewData
                };

                ControllerContext settingsContext = new ControllerContext(new RequestContext(new EmptyHttpContext(), context.RouteData), context.Controller);

                settingsResult.ExecuteResult(settingsContext);

                return settingsResult.Settings;
            }
            catch (HttpException)
            {
                throw;
            }
        }

        class EmptyHttpContext : HttpContextBase
        {
            Hashtable _items = new Hashtable();
            HttpRequestBase _request = new EmptyHttpRequest();
            HttpResponseBase _response = new EmptyHttpResponse();
            PageInstrumentationService _pageInstrumentation = new PageInstrumentationService();

            public override System.Web.Caching.Cache Cache
            {
                get
                {
                    return HttpRuntime.Cache;
                }
            }

            public override IDictionary Items
            {
                get
                {
                    return _items;
                }
            }

            public override HttpRequestBase Request
            {
                get
                {
                    return _request;
                }
            }

            public override HttpResponseBase Response
            {
                get
                {
                    return _response;
                }
            }

            public override PageInstrumentationService PageInstrumentation
            {
                get
                {
                    return _pageInstrumentation;
                }
            }

            //http://stackoverflow.com/a/24348822/65611
            public override object GetService(Type serviceType)
            {
                return DependencyResolver.Current.GetService(serviceType);
            }
        }

        class EmptyHttpRequest : HttpRequestBase
        {
            HttpCookieCollection _cookies = new HttpCookieCollection();
            NameValueCollection _serverVariables = new NameValueCollection();
            EmptyHttpBrowserCapabilities _browser = new EmptyHttpBrowserCapabilities();

            public override HttpCookieCollection Cookies
            {
                get
                {
                    return _cookies;
                }
            }

            public override string UserAgent
            {
                get
                {
                    return null;
                }
            }

            public override HttpBrowserCapabilitiesBase Browser
            {
                get
                {
                    return _browser;
                }
            }

            public override bool IsLocal
            {
                get
                {
                    return true;
                }
            }

            public override string ApplicationPath
            {
                get
                {
                    return "/";
                }
            }

            public override NameValueCollection ServerVariables
            {
                get
                {
                    return _serverVariables;
                }
            }

            public override Uri Url
            {
                get
                {
                    return new Uri("http://localhost/");
                }
            }
        }

        class EmptyHttpResponse : HttpResponseBase
        {
            HttpCookieCollection _cookies = new HttpCookieCollection();

            public override HttpCookieCollection Cookies
            {
                get
                {
                    return _cookies;
                }
            }

            public override string ApplyAppPathModifier(string virtualPath)
            {
                return virtualPath;
            }
        }

        class EmptyHttpBrowserCapabilities : HttpBrowserCapabilitiesBase
        {
            public override bool IsMobileDevice
            {
                get
                {
                    return false;
                }
            }
        }
    }
}
#endif