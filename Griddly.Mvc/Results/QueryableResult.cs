using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Web.Helpers;
using Griddly.Mvc.Linq.Dynamic;

namespace Griddly.Mvc.Results
{
    public class QueryableResult<T> : GriddlyResult<T>
    {
        IQueryable<T> _result;
        Func<IQueryable<T>, IQueryable<T>> _massage = null;
        string _finalSortField;

        static readonly bool _typeHasId = typeof(T).GetProperty("Id") != null;

        public QueryableResult(IQueryable<T> result, string viewName = null, Func<IQueryable<T>, IQueryable<T>> massage = null, string finalSortField = null)
            : base(viewName)
        {
            _result = result;
            _massage = massage;
            _finalSortField = finalSortField;

            if (_finalSortField == null && _typeHasId)
                _finalSortField = "Id";
        }

        public override IEnumerable<T> GetAll(SortField[] sortFields)
        {
            IQueryable<T> sortedQuery = ApplySortFields(_result, sortFields, _finalSortField);

            if (_massage != null)
                sortedQuery = _massage(sortedQuery);

            return sortedQuery;
        }

        public override IList<T> GetPage(int pageNumber, int pageSize, SortField[] sortFields)
        {
            IQueryable<T> sortedQuery = ApplySortFields(_result, sortFields, _finalSortField);

            if (_massage != null)
                sortedQuery = _massage(sortedQuery);

            return sortedQuery.Skip(pageNumber * pageSize).Take(pageSize).ToList();
        }

        public override void PopulateSummaryValues(GriddlySettings<T> settings)
        {
            // Only works for linq to objects
            //List<GriddlyColumn> summaryColumns = settings.Columns.Where(x => x.SummaryFunction != null).ToList();

            //if (summaryColumns.Any())
            //{
            //    StringBuilder aggregateExpression = new StringBuilder();

            //    aggregateExpression.Append("new (");

            //    for (int i = 0; i < summaryColumns.Count; i++)
            //    {
            //        if (i > 0)
            //            aggregateExpression.Append(", ");

            //        GriddlyColumn col = summaryColumns[i];

            //        aggregateExpression.AppendFormat("{0}({1}) AS _a{2}", col.SummaryFunction, col.ExpressionString, i);
            //    }

            //    aggregateExpression.Append(")");

            //    var query = _result.GroupBy(x => 1).Select(aggregateExpression.ToString());
            //    var item = query.Cast<object>().Single();
            //    var type = item.GetType();

            //    for (int i = 0; i < summaryColumns.Count; i++)
            //        summaryColumns[i].SummaryValue = type.GetProperty("_a" + i).GetValue(item);
            //}

            // TODO: figure out how to get this in one query
            foreach (GriddlyColumn c in settings.Columns.Where(x => x.SummaryFunction != null))
                PopulateSummaryValue(c);
        }

        internal void PopulateSummaryValue(GriddlyColumn c)
        {
            // NOTE: Also in GriddlyExtensions.SimpleGriddly
            switch (c.SummaryFunction.Value)
            {
                case SummaryAggregateFunction.Sum:
                case SummaryAggregateFunction.Average:
                case SummaryAggregateFunction.Min:
                case SummaryAggregateFunction.Max:
                    try
                    {
                        c.SummaryValue = _result.Aggregate(c.SummaryFunction.Value.ToString(), c.ExpressionString);
                    }
                    catch (Exception ex) when (ex.InnerException is ArgumentNullException)
                    {
                        c.SummaryValue = null;
                    }
                    break;

                default:
                    throw new InvalidOperationException(string.Format("Unknown summary function {0} for column {1}.", c.SummaryFunction, c.ExpressionString));
            }
        }

        public override IEnumerable<P> GetAllForProperty<P>(string propertyName, SortField[] sortFields)
        {
            return ApplySortFields(_result, sortFields, _finalSortField)
                .Select<P>(propertyName, null);
        }

        public override long GetCount()
        {
            return _result.Count();
        }

        protected static IQueryable<T> ApplySortFields(IQueryable<T> source, SortField[] sortFields, string finalSortField)
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

            if (finalSortField != null)
            {
                if (sortedQuery == null)
                    sortedQuery = source is IOrderedQueryable<T> sourceOrdered && typeof(IOrderedQueryable<T>).IsAssignableFrom(source.Expression.Type)
                        ? ThenByDescending(sourceOrdered, finalSortField) : OrderByDescending(source, finalSortField);
                else
                    sortedQuery = ThenByDescending(sortedQuery, finalSortField);
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
                PropertyInfo pi = type.GetProperty(prop, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
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
