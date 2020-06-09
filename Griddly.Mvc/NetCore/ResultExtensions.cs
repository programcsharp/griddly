#if !NET45

using System.Linq;
using System;
using Griddly.Mvc.Results;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Data;

namespace Griddly.Mvc
{
    public static class ResultExtensions
    {
        public static MapQueryableResult<TIn, TOut> GriddlyMapQueryable<TIn, TOut>(this Controller _this, IQueryable<TIn> result, Func<IEnumerable<TIn>, IEnumerable<TOut>> map, string viewName = null, Func<IQueryable<TIn>, IQueryable<TIn>> massage = null)
        {
            return new MapQueryableResult<TIn, TOut>(result, map, viewName, massage)
            {
                ViewData = _this.ViewData
            };
        }
        public static QueryableResult<T> GriddlyQueryable<T> (this Controller _this, IQueryable<T> result, string viewName = null, Func<IQueryable<T>, IQueryable<T>> massage = null, string finalSortField = null)
        {
            return new QueryableResult<T>(result, viewName, massage, finalSortField)
            {
                ViewData = _this.ViewData
            };
        }
        public static DapperResult<T> GriddlyDapperSql<T>(this Controller _this, Func<IDbConnection> getConnection, string sql, object param, Func<IDbConnection, IDbTransaction, string, object, IEnumerable<T>> map, Action<IDbConnection, IDbTransaction, IList<T>> massage, bool fixedSort, Func<IDbTransaction> getTransaction, string outerSqlTemplate, int? commandTimout = null)
        {
            return new DapperSql2012Result<T>(getConnection, sql, param, map, massage, fixedSort, getTransaction, outerSqlTemplate, commandTimout)
            {
                ViewData = _this.ViewData
            };
        }
    }
}

#endif