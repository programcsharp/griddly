using Griddly.Mvc.Linq.Dynamic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Web.Helpers;
using System.Web.Mvc;

namespace Griddly.Mvc
{
    public abstract class GriddlyResult : ActionResult
    {
        public SortField[] GetSortFields(NameValueCollection items)
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
    }

    public abstract class GriddlyResult<T> : GriddlyResult
    {
        public string ViewName { get; set; }

        public GriddlyResult(string viewName = null)
        {
            ViewName = viewName;
        }

        public override void ExecuteResult(ControllerContext context)
        {
            int pageNumber;
            int pageSize;
            SortField[] sortFields = null;
            GriddlyExportFormat exportFormatValue;
            GriddlyExportFormat? exportFormat;

            NameValueCollection items = new NameValueCollection(context.RequestContext.HttpContext.Request.Params);

            if (!int.TryParse(items["pageNumber"], out pageNumber))
                pageNumber = 0;

            if (!int.TryParse(items["pageSize"], out pageSize))
                pageSize = 20;

            if (Enum.TryParse(items["exportFormat"], true, out exportFormatValue))
                exportFormat = exportFormatValue;
            else
                exportFormat = null;

            sortFields = GetSortFields(items);

            GriddlySettings settings = null;

            if (context.IsChildAction)
            {
                settings = GriddlySettingsResult.GetSettings(context, ViewName);

                if (GriddlySettings.OnGriddlyResultExecuting != null)
                    GriddlySettings.OnGriddlyResultExecuting(settings, context);

                // TODO: should we always pull sort fields?
                if (!sortFields.Any())
                    sortFields = settings.DefaultSort;

                if (settings.PageSize > settings.MaxPageSize)
                    settings.PageSize = settings.MaxPageSize;

                if (settings.PageSize != null)
                    pageSize = settings.PageSize.Value;
            }

            if (exportFormat == null)
            {
                IList<T> page = GetPage(pageNumber, pageSize, sortFields);

                GriddlyResultPage<T> result = new GriddlyResultPage<T>()
                {
                    Data = page,
                    Count = page.Count,
                    PageNumber = pageNumber,
                    Total = GetCount(),
                    PageSize = pageSize,
                    SortFields = sortFields,
                    Settings = settings,
                    PopulateSummaryValues = PopulateSummaryValues
                };

                context.RequestContext.HttpContext.Response.Headers["X-Griddly-Count"] = result.Total.ToString();
                context.RequestContext.HttpContext.Response.Headers["X-Griddly-CurrentPageSize"] = result.Count.ToString();

                PartialViewResult view = new PartialViewResult()
                {
                    ViewData = new ViewDataDictionary(result),
                    ViewName = ViewName
                };

                foreach (KeyValuePair<string, object> value in context.Controller.ViewData.Where(x => x.Key != "_isGriddlySettingsRequest"))
                    view.ViewData[value.Key] = value.Value;

                if (context.IsChildAction)
                {
                    foreach (KeyValuePair<string, object> value in context.ParentActionViewContext.ViewData)
                        view.ViewData[value.Key] = value.Value;
                }

                view.ExecuteResult(context);
            }
            else
            {
                settings = GriddlySettingsResult.GetSettings(context, ViewName);

                settings.Columns.RemoveAll(x => x is GriddlySelectColumn);

                ActionResult result;

                string fileName = settings.Title ?? context.Controller.ControllerContext.RouteData.Values["controller"].ToString();

                if (fileName != null)
                {
                    foreach (char c in System.IO.Path.GetInvalidFileNameChars())
                        fileName = fileName.Replace(c, '_');
                }

                if (exportFormat == GriddlyExportFormat.Custom)
                {
                    result = GriddlySettings.HandleCustomExport(this, items);
                }
                else
                {
                    var records = GetAll(sortFields);
                    if (exportFormat == GriddlyExportFormat.Xlsx)
                    {
                        result = new GriddlyExcelResult<T>(records, settings, fileName, items["exportName"]);
                    }
                    else // if (exportFormat == GriddlyExportFormat.Csv || exportFormat == GriddlyExportFormat.Tsv)
                    {
                        result = new GriddlyCsvResult<T>(records, settings, fileName, exportFormat.Value, items["exportName"]);
                    }
                }
                
                result.ExecuteResult(context);
            }
        }

        public abstract IEnumerable<T> GetAll(SortField[] sortFields);
        
        public abstract IList<T> GetPage(int pageNumber, int pageSize, SortField[] sortFields);
        
        public abstract void PopulateSummaryValues(GriddlySettings<T> settings);

        public abstract long GetCount();
    }

    public enum GriddlyExportFormat
    {
        Xlsx,
        Csv,
        Tsv,
        Custom
    }
}
