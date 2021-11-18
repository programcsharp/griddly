using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
#if NETFRAMEWORK
using Griddly.Mvc.Linq.Dynamic;
using System.Web.Helpers;
using System.Web.Mvc;
#else
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewEngines;
#endif

namespace Griddly.Mvc
{
    public abstract class GriddlyResult : ActionResult
    {
        public static SortField[] GetSortFields(NameValueCollection items)
        {
            return items.AllKeys
                .Where(x => x != null && x.StartsWith("sortFields["))
                .Select(x =>
                {
                    int pos = x.IndexOf(']', "sortFields[".Length);

                    return new
                    {
                        Index = int.Parse(x.Substring("sortFields[".Length, pos - "sortFields[".Length)),
                        Field = x.Substring(pos + 2, x.Length - pos - 2 - 1),
                        Direction = (SortDirection)Enum.Parse(typeof(SortDirection), items[x])
                    };

                })
                .OrderBy(x => x.Index)
                .Select(x => new SortField()
                {
                    Field = x.Field,
                    Direction = x.Direction
                })
                .ToArray();
        }

        public abstract IEnumerable GetAllNonGeneric(SortField[] sortFields);

        public abstract IList GetPageNonGeneric(int pageNumber, int pageSize, SortField[] sortFields);

        public abstract void PopulateSummaryValuesNonGeneric(GriddlySettings settings);

        public abstract long GetCount();

        public abstract IEnumerable<P> GetAllForProperty<P>(string propertyName, SortField[] sortFields);
    }

    public abstract class GriddlyResult<T> : GriddlyResult
    {
        public string ViewName { get; set; }
#if NETCOREAPP
        public ViewDataDictionary ViewData { get; private set; }
#endif

        public GriddlyResult (
#if NETCOREAPP
            ViewDataDictionary viewData,
#endif
            string viewName = null)
        {
            ViewName = viewName;
#if NETCOREAPP
            ViewData = viewData;
#endif
        }

#if NETFRAMEWORK
        public override void ExecuteResult(ControllerContext context)
        {
#else
        public override async Task ExecuteResultAsync(ActionContext context)
        {
            if (ViewData == null)
                throw new ArgumentNullException("ViewData", "Use the controller extension method to create the result, or specify ViewData manually");
#endif
            if (GriddlySettings.DisableHistoryParameters)
            {
#if NETFRAMEWORK
                // not using history, so make sure we don't get half junk in FF etc from back buttons
                context.RequestContext.HttpContext.Response.Cache.SetNoServerCaching();
                context.RequestContext.HttpContext.Response.Cache.SetNoStore();
#else
                context.HttpContext.Response.Headers["Cache-Control"] = "no-cache, no-store";
#endif
            }

#if NETFRAMEWORK
            var griddlyContext = context.Controller.GetOrCreateGriddlyContext();
            var httpContext = context.HttpContext;
            GriddlySettings settings = GriddlySettingsResult.GetSettings(context, ViewName);
#else
            var griddlyContext = context.GetOrCreateGriddlyContext();
            var httpContext = context.HttpContext;
            GriddlySettings settings = await GriddlySettingsResult.GetSettings(context, ViewName, ViewData);
#endif

            if (griddlyContext.SortFields?.Length > 0)
            {
                // white list for sql injection
                griddlyContext.SortFields = griddlyContext.SortFields
                    .Where(x => settings.Columns.Any(y => y.ExpressionString == x.Field))
                    .ToArray();
            }

#if NETFRAMEWORK
            if (context.IsChildAction)
            {
                //settings = GriddlySettingsResult.GetSettings(context, ViewName); - this looks redundant to me.
#else
            if (context.HttpContext.IsChildAction())
            {
                //settings = await GriddlySettingsResult.GetSettings(context, ViewName);
#endif

                GriddlySettings.OnGriddlyResultExecuting?.Invoke(settings, context);

                // TODO: should we always pull sort fields?
                if (griddlyContext.SortFields?.Any() != true)
                    griddlyContext.SortFields = settings.DefaultSort;

                if (settings.PageSize > settings.MaxPageSize)
                    settings.PageSize = settings.MaxPageSize;

                if (settings.PageSize != null)
                    griddlyContext.PageSize = settings.PageSize.Value;
            }

            GriddlyParameterAttribute.AddCookieDataIfNeeded(griddlyContext, context.HttpContext);

            if (griddlyContext.ExportFormat == null)
            {
                GriddlySettings.OnGriddlyPageExecuting?.Invoke(settings, griddlyContext, context);

                IList<T> page = GetPage(griddlyContext.PageNumber, griddlyContext.PageSize, griddlyContext.SortFields);

                GriddlyResultPage<T> result = new GriddlyResultPage<T>()
                {
                    Data = page,
                    Count = page.Count,
                    PageNumber = griddlyContext.PageNumber,
                    Total = GetCount(),
                    PageSize = griddlyContext.PageSize,
                    SortFields = griddlyContext.SortFields,
                    Settings = settings,
                    PopulateSummaryValues = PopulateSummaryValues
                };

                httpContext.Response.Headers["X-Griddly-Count"] = result.Total.ToString();
                httpContext.Response.Headers["X-Griddly-CurrentPageSize"] = result.Count.ToString();

                PartialViewResult view = new PartialViewResult()
                {
#if NETFRAMEWORK
                    ViewData = new ViewDataDictionary(result),
#else
                    ViewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary()) { Model = result },
#endif
                    ViewName = ViewName,
                };

#if NETFRAMEWORK
                foreach (KeyValuePair<string, object> value in context.Controller.ViewData.Where(x => x.Key != "_isGriddlySettingsRequest"))
                    view.ViewData[value.Key] = value.Value;

                if (context.IsChildAction)
                {
                    foreach (KeyValuePair<string, object> value in context.ParentActionViewContext.ViewData)
                        view.ViewData[value.Key] = value.Value;
                }
                view.ExecuteResult(context);
#else
                if (ViewData != null)
                {
                    foreach (KeyValuePair<string, object> value in ViewData.Where(x => x.Key != "_isGriddlySettingsRequest"))
                        view.ViewData[value.Key] = value.Value;
                }

                if (context.HttpContext.IsChildAction())
                {
                    foreach (KeyValuePair<string, object> value in context.HttpContext.ParentActionViewContext().ViewData)
                        view.ViewData[value.Key] = value.Value;
                }
                await view.ExecuteResultAsync(context);
#endif
            }
            else
            {
#if NETFRAMEWORK
                var routeData = context.Controller.ControllerContext.RouteData;
                var parms = context.HttpContext.Request.Params;
#else
                var routeData = context.RouteData;
                var parms = context.HttpContext.Request.GetParams();
#endif

                settings.Columns.RemoveAll(x => x is GriddlySelectColumn);

                ActionResult result;

                string fileName = settings.Title ?? routeData.Values["controller"].ToString();

                if (fileName != null)
                {
                    foreach (char c in System.IO.Path.GetInvalidFileNameChars())
                        fileName = fileName.Replace(c, '_');
                }

                NameValueCollection items = new NameValueCollection(parms);

                if (griddlyContext.ExportFormat == GriddlyExportFormat.Custom)
                {
                    result = GriddlySettings.HandleCustomExport(this, items, context);
                }
                else
                {
                    IEnumerable<T> records = GetAll(griddlyContext.SortFields);

                    if (GriddlySettings.OnGriddlyExportExecuting != null)
                        records = GriddlySettings.OnGriddlyExportExecuting(records, settings).Cast<T>();

                    if (griddlyContext.ExportFormat == GriddlyExportFormat.Xlsx)
                    {
                        result = new GriddlyExcelResult<T>(records, settings, fileName, items["exportName"]);
                    }
                    else // if (exportFormat == GriddlyExportFormat.Csv || exportFormat == GriddlyExportFormat.Tsv)
                    {
                        result = new GriddlyCsvResult<T>(records, settings, fileName, griddlyContext.ExportFormat.Value, items["exportName"]);
                    }
                }

#if NETFRAMEWORK
                result.ExecuteResult(context);
#else
                await result.ExecuteResultAsync(context);
#endif
            }
        }

        public abstract IEnumerable<T> GetAll(SortField[] sortFields);
        
        public abstract IList<T> GetPage(int pageNumber, int pageSize, SortField[] sortFields);
        
        public abstract void PopulateSummaryValues(GriddlySettings<T> settings);

        public override IEnumerable<P> GetAllForProperty<P>(string propertyName, SortField[] sortFields)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable GetAllNonGeneric(SortField[] sortFields)
        {
            return GetAll(sortFields);
        }
               
        public override IList GetPageNonGeneric(int pageNumber, int pageSize, SortField[] sortFields)
        {
            return (IList)(GetPage(pageNumber, pageSize, sortFields).ToList());
        }
               
        public override void PopulateSummaryValuesNonGeneric(GriddlySettings settings)
        {
            PopulateSummaryValues((GriddlySettings<T>)settings);
        }
    }
}
