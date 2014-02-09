using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Web.Mvc;

namespace Griddly.Mvc
{
    public abstract class GriddlyResult : ActionResult
    {

    }

    public class GriddlyResult<T> : GriddlyResult
    {
        IQueryable<T> _result;

        public string ViewName { get; set; }

        public GriddlyResult(IQueryable<T> result, string viewName = null)
        {
            _result = result;
            ViewName = viewName;
        }

        public override void ExecuteResult(ControllerContext context)
        {
            int pageNumber;
            int pageSize;
            string[] sortFields = null;
            GriddlyExportFormat exportFormatValue;
            GriddlyExportFormat? exportFormat;

            NameValueCollection items = new NameValueCollection(context.RequestContext.HttpContext.Request.Params);

            if (!int.TryParse(items["pageNumber"], out pageNumber))
                pageNumber = 0;
            if (!int.TryParse(items["pageSize"], out pageSize))
                pageSize = 20;
            if (!string.IsNullOrWhiteSpace(items["sortFields"]))
                sortFields = items["sortFields"].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToArray();
            if (Enum.TryParse(items["exportFormat"], true, out exportFormatValue))
                exportFormat = exportFormatValue;
            else
                exportFormat = null;

            if (context.IsChildAction)
            {
                GriddlySettings settings = GriddlySettingsResult.GetSettings(context, ViewName);

                if (sortFields == null)
                {
                    List<string> sortFieldList = new List<string>();

                    foreach (GriddlyColumn column in settings.Columns)
                    {
                        if (!string.IsNullOrWhiteSpace(column.DefaultSort))
                            sortFieldList.Add(column.SortField + " " + column.DefaultSort);
                    }

                    if (sortFieldList.Count > 0)
                        sortFields = sortFieldList.ToArray();
                }

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
                    SortFields = sortFields
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
                GriddlySettings settings = GriddlySettingsResult.GetSettings(context, ViewName);

                settings.Columns.RemoveAll(x => x is GriddlySelectColumn);

                ActionResult result;

                if (exportFormat == GriddlyExportFormat.Xlsx)
                    result = new GriddlyExcelResult<T>(GetAll(sortFields), settings, context.Controller.ControllerContext.RouteData.Values["controller"].ToString());
                else// if (exportFormat == GriddlyExportFormat.Csv || exportFormat == GriddlyExportFormat.Tsv)
                    result = new GriddlyCsvResult<T>(GetAll(sortFields), settings, context.Controller.ControllerContext.RouteData.Values["controller"].ToString(), exportFormat.Value);

                result.ExecuteResult(context);
            }
        }

        protected virtual IList<T> GetAll(string[] sortFields)
        {
            IQueryable<T> sortedQuery = ApplySortFields(_result, sortFields);

            return sortedQuery.ToList();
        }

        protected virtual IList<T> GetPage(int pageNumber, int pageSize, string[] sortFields)
        {
            IQueryable<T> sortedQuery = ApplySortFields(_result, sortFields);

            return sortedQuery.Skip(pageNumber * pageSize).Take(pageSize).ToList();
        }

        protected virtual long GetCount()
        {
            return _result.Count();
        }

        static IQueryable<T> ApplySortFields(IQueryable<T> source, string[] sortFields)
        {
            IOrderedQueryable<T> sortedQuery = null;

            if (sortFields != null)
            {
                for (int i = 0; i < sortFields.Length; i++)
                {
                    string sortField = sortFields[i];
                    string sortOrder = "ASC";

                    if (sortField.EndsWith(" desc", StringComparison.InvariantCultureIgnoreCase))
                    {
                        sortField = sortField.Substring(0, sortField.Length - " desc".Length);
                        sortOrder = "DESC";
                    }
                    else if (sortField.EndsWith(" asc", StringComparison.InvariantCultureIgnoreCase))
                    {
                        sortField = sortField.Substring(0, sortField.Length - " asc".Length);
                        sortOrder = "ASC";
                    }

                    if (sortOrder == "ASC")
                    {
                        if (i == 0)
                            sortedQuery = OrderBy(source, sortField);
                        else
                            sortedQuery = ThenBy(sortedQuery, sortField);
                    }
                    else
                    {
                        if (i == 0)
                            sortedQuery = OrderByDescending(source, sortField);
                        else
                            sortedQuery = ThenByDescending(sortedQuery, sortField);
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
        Tsv
    }
}
