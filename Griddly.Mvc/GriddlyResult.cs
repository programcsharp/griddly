using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;

namespace Griddly.Mvc
{
    public abstract class GriddlyResult : ActionResult
    {
        public SortField[] GetSortFields(NameValueCollection items)
        {
            return items.AllKeys
                .Where(x => x.StartsWith("sortFields["))
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

    public class GriddlyResult<T> : GriddlyResult
    {
        IQueryable<T> _result;
        Func<IQueryable<T>, IQueryable<T>> _massage = null;

        public string ViewName { get; set; }

        public GriddlyResult(IQueryable<T> result, string viewName = null, Func<IQueryable<T>, IQueryable<T>> massage = null)
        {
            _result = result;
            ViewName = viewName;
            _massage = massage;
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

                // TODO: should we always pull sort fields?
                if (!sortFields.Any())
                    sortFields = settings.GetDefaultSort();

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
                    Settings = settings
                };

                context.RequestContext.HttpContext.Response.Headers["X-Griddly-Count"] = result.Total.ToString();
                context.RequestContext.HttpContext.Response.Headers["X-Griddly-CurrentPageSize"] = result.Count.ToString();

                PartialViewResult view = new PartialViewResult()
                {
                    ViewData = new ViewDataDictionary(result),
                    ViewName = ViewName
                };

                foreach (KeyValuePair<string, object> value in context.Controller.ViewData.Where(x => !x.Key.StartsWith("_")))
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

                var records = GetAll(sortFields);
                if (exportFormat == GriddlyExportFormat.Custom)
                {
                    string report = items["reportType"];
                    result = GriddlySettings.HandleCustomReport(report, records);
                }
                else if (exportFormat == GriddlyExportFormat.Xlsx)
                {
                    result = new GriddlyExcelResult<T>(records, settings, fileName);
                }
                else // if (exportFormat == GriddlyExportFormat.Csv || exportFormat == GriddlyExportFormat.Tsv)
                {
                    result = new GriddlyCsvResult<T>(records, settings, fileName, exportFormat.Value);
                }

                result.ExecuteResult(context);
            }
        }

        public virtual IEnumerable<T> GetAll(SortField[] sortFields)
        {
            IQueryable<T> sortedQuery = ApplySortFields(_result, sortFields);

            return sortedQuery;
        }

        public virtual IList<T> GetPage(int pageNumber, int pageSize, SortField[] sortFields)
        {
            IQueryable<T> sortedQuery = ApplySortFields(_result, sortFields);

            if (_massage != null)
                sortedQuery = _massage(sortedQuery);

            return sortedQuery.Skip(pageNumber * pageSize).Take(pageSize).ToList();
        }

        public virtual long GetCount()
        {
            return _result.Count();
        }

        static IQueryable<T> ApplySortFields(IQueryable<T> source, SortField[] sortFields)
        {
            IOrderedQueryable<T> sortedQuery = null;

            if (sortFields != null)
            {
                for (int i = 0; i < sortFields.Length; i++)
                {
                    SortField sortField = sortFields[i];

                    if (sortField.Direction == SortDirection.Ascending)
                    {
                        if (i == 0)
                            sortedQuery = OrderBy(source, sortField.Field);
                        else
                            sortedQuery = ThenBy(sortedQuery, sortField.Field);
                    }
                    else
                    {
                        if (i == 0)
                            sortedQuery = OrderByDescending(source, sortField.Field);
                        else
                            sortedQuery = ThenByDescending(sortedQuery, sortField.Field);
                    }
                }
            }

            return sortedQuery ?? source;
        }

        // http://stackoverflow.com/a/233505/8037
        static IOrderedQueryable<T> OrderBy(IQueryable<T> source, string property)
        {
            return ApplyOrder(source, property, "OrderBy");
        }

        static IOrderedQueryable<T> OrderByDescending(IQueryable<T> source, string property)
        {
            return ApplyOrder(source, property, "OrderByDescending");
        }

        static IOrderedQueryable<T> ThenBy(IOrderedQueryable<T> source, string property)
        {
            return ApplyOrder(source, property, "ThenBy");
        }

        static IOrderedQueryable<T> ThenByDescending(IOrderedQueryable<T> source, string property)
        {
            return ApplyOrder(source, property, "ThenByDescending");
        }

        static IOrderedQueryable<T> ApplyOrder(IQueryable<T> source, string property, string methodName)
        {
            string[] props = property.Split('.');
            Type type = typeof(T);
            ParameterExpression arg = Expression.Parameter(type, "x");
            Expression expr = arg;
            foreach (string prop in props)
            {
                // use reflection (not ComponentModel) to mirror LINQ
                PropertyInfo pi = type.GetProperty(prop);
                expr = Expression.Property(expr, pi);
                type = pi.PropertyType;
            }
            Type delegateType = typeof(Func<,>).MakeGenericType(typeof(T), type);
            LambdaExpression lambda = Expression.Lambda(delegateType, expr, arg);

            object result = typeof(Queryable).GetMethods().Single(
                    method => method.Name == methodName
                            && method.IsGenericMethodDefinition
                            && method.GetGenericArguments().Length == 2
                            && method.GetParameters().Length == 2)
                    .MakeGenericMethod(typeof(T), type)
                    .Invoke(null, new object[] { source, lambda });
            return (IOrderedQueryable<T>)result;
        }
    }

    public enum GriddlyExportFormat
    {
        Xlsx,
        Csv,
        Tsv,
        Custom
    }
}
