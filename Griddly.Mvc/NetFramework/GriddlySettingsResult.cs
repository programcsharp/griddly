#if NETFRAMEWORK
using System.IO;
using System.Security.Authentication.ExtendedProtection;
using System.Security.Principal;
using System.Threading;
using System.Web.Instrumentation;

namespace Griddly.Mvc;

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
            var httpContext = new EmptyHttpContext(context.HttpContext.Request);
            var requestContext = new RequestContext(httpContext, context.RouteData);
            httpContext.Request.RequestContext = requestContext;
            ControllerContext settingsContext = new ControllerContext(requestContext, context.Controller);

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
        public EmptyHttpContext(HttpRequestBase request)
        {
            _request = new EmptyHttpRequest(request);
        }

        Hashtable _items = new Hashtable();
        HttpRequestBase _request;
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
        public EmptyHttpRequest (HttpRequestBase originalRequest)
        {
            _originalRequest = originalRequest;
        }
        private readonly HttpRequestBase _originalRequest;

        public override bool IsAuthenticated => _originalRequest.IsAuthenticated;
        public override bool IsLocal => _originalRequest.IsLocal;
        public override bool IsSecureConnection => _originalRequest.IsSecureConnection;
        public override WindowsIdentity LogonUserIdentity => _originalRequest.LogonUserIdentity;
        public override NameValueCollection Params => _originalRequest.Params;
        public override string Path => _originalRequest.Path;
        public override string PathInfo => _originalRequest.PathInfo;
        public override string PhysicalApplicationPath => _originalRequest.PhysicalApplicationPath;
        public override string PhysicalPath => _originalRequest.PhysicalPath;
        public override string RawUrl => _originalRequest.RawUrl;
        public override ReadEntityBodyMode ReadEntityBodyMode => _originalRequest.ReadEntityBodyMode;
        public override NameValueCollection ServerVariables => _originalRequest.ServerVariables;
        public override CancellationToken TimedOutToken => _originalRequest.TimedOutToken;
        public override ITlsTokenBindingInfo TlsTokenBindingInfo => _originalRequest.TlsTokenBindingInfo;
        public override int TotalBytes => _originalRequest.TotalBytes;
        public override UnvalidatedRequestValuesBase Unvalidated => _originalRequest.Unvalidated;
        public override Uri Url => _originalRequest.Url;
        public override Uri UrlReferrer => _originalRequest.UrlReferrer;
        public override string UserAgent => _originalRequest.UserAgent;
        public override string[] UserLanguages => _originalRequest.UserLanguages;
        public override string UserHostAddress => _originalRequest.UserHostAddress;
        public override string UserHostName => _originalRequest.UserHostName;
        public override NameValueCollection Headers => _originalRequest.Headers;
        public override string HttpMethod => _originalRequest.HttpMethod;
        public override NameValueCollection Form => _originalRequest.Form;
        public override HttpFileCollectionBase Files => _originalRequest.Files;
        public override NameValueCollection QueryString => _originalRequest.QueryString;
        public override string ApplicationPath => _originalRequest.ApplicationPath;
        public override string AnonymousID => _originalRequest.AnonymousID;
        public override string AppRelativeCurrentExecutionFilePath => _originalRequest.AppRelativeCurrentExecutionFilePath;
        public override HttpBrowserCapabilitiesBase Browser => _originalRequest.Browser;
        public override ChannelBinding HttpChannelBinding => _originalRequest.HttpChannelBinding;
        public override HttpClientCertificate ClientCertificate => _originalRequest.ClientCertificate;
        public override string[] AcceptTypes => _originalRequest.AcceptTypes;
        public override int ContentLength => _originalRequest.ContentLength;
        public override HttpCookieCollection Cookies => _originalRequest.Cookies;
        public override string CurrentExecutionFilePath => _originalRequest.CurrentExecutionFilePath;
        public override string CurrentExecutionFilePathExtension => _originalRequest.CurrentExecutionFilePathExtension;
        public override string FilePath => _originalRequest.FilePath;
        public override string RequestType { get { return _originalRequest.RequestType; } set { } }
        public override string ContentType { get { return _originalRequest.ContentType; } set { } }
        public override System.Text.Encoding ContentEncoding { get { return _originalRequest.ContentEncoding; } set { } }
        public override RequestContext RequestContext { get; set; }
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
#endif