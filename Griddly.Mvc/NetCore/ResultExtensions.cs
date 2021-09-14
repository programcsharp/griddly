#if !NET45_OR_GREATER

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
            return new MapQueryableResult<TIn, TOut>(result, _this.ViewData, map, viewName, massage);
        }
        public static QueryableResult<T> GriddlyQueryable<T> (this Controller _this, IQueryable<T> result, string viewName = null, Func<IQueryable<T>, IQueryable<T>> massage = null, string finalSortField = null)
        {
            return new QueryableResult<T>(result, _this.ViewData, viewName, massage, finalSortField);
        }
        public static DapperResult<T> GriddlyDapperSql<T>(this Controller _this, Func<IDbConnection> getConnection, string sql, object param, Func<IDbConnection, IDbTransaction, string, object, IEnumerable<T>> map, Action<IDbConnection, IDbTransaction, IList<T>> massage, bool fixedSort, Func<IDbTransaction> getTransaction, string outerSqlTemplate, int? commandTimout = null)
        {
            return new DapperSql2012Result<T>(getConnection, sql, param, _this.ViewData, map, massage, fixedSort, getTransaction, outerSqlTemplate, commandTimout);
        }
    }
}

#endif