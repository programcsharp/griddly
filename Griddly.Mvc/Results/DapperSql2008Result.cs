using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

#if NETCOREAPP
using Microsoft.AspNetCore.Mvc.ViewFeatures;
#endif

namespace Griddly.Mvc.Results
{
    public class DapperSql2008Result<T> : DapperResult<T>
    {
        public DapperSql2008Result(Func<IDbConnection> getConnection, string sql, object param,
#if NETCOREAPP
            ViewDataDictionary viewData,
#endif
            Func<IDbConnection, IDbTransaction, string, object, IEnumerable<T>> map = null, Action<IDbConnection, IDbTransaction, IList<T>> massage = null, bool fixedSort = false, Func<IDbTransaction> getTransaction = null, string outerSqlTemplate = "{0}", int? commandTimeout = null)
            : base(getConnection, sql, param,
#if NETCOREAPP
                  viewData,
#endif
                  map, massage, fixedSort, getTransaction, outerSqlTemplate, commandTimeout)
        { }

        public override IList<T> GetPage(int pageNumber, int pageSize, SortField[] sortFields)
        {
            string format;

            if (!_hasOverallCount || _sql.IndexOf("OverallCount", StringComparison.InvariantCultureIgnoreCase) != -1)
            {
                format = @"
;WITH _data AS (
    {0}
)
SELECT * FROM (
    SELECT *, ROW_NUMBER() OVER (ORDER BY {1}) AS row_num FROM _data) x
WHERE row_num BETWEEN {2} AND {3} " + (_fixedSort ? "" : "ORDER BY {1}");
            }
            else
            {
                // TODO: use dapper multimap Query<T, Dictionary<string, object>> to map all summary values in one go
                format = @"
;WITH _data AS (
    {0}
),
    _count AS (
        SELECT COUNT(0) AS OverallCount FROM _data
)
SELECT * FROM (
    SELECT *, ROW_NUMBER() OVER (ORDER BY {1}) AS row_num FROM _data CROSS APPLY _count) x
WHERE row_num BETWEEN {2} AND {3} " + (_fixedSort ? "" : "ORDER BY {1}");
            }

            string sql = string.Format(_outerSqlTemplate,
                string.Format(format, _sql, BuildSortClause(sortFields) ?? "CURRENT_TIMESTAMP", (pageNumber * pageSize) + 1, (pageNumber * pageSize) + pageSize));

            return ExecuteQuery(sql);
        }

        public override IEnumerable<T> GetAll(SortField[] sortFields)
        {
            string sql = string.Format(_outerSqlTemplate,
                _fixedSort ? _sql : string.Format("{0} ORDER BY {1}", _sql, BuildSortClause(sortFields) ?? "CURRENT_TIMESTAMP"));

            return ExecuteQuery(sql);
        }
    }
}
