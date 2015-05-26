using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Web.Helpers;

namespace Griddly.Mvc.Results
{
    public class DapperSql2012Result<T> : DapperResult<T>
    {
        public DapperSql2012Result(Func<IDbConnection> getConnection, string sql, object param, Func<IDbConnection, IDbTransaction, string, object, IEnumerable<T>> map = null, Action<IDbConnection, IDbTransaction, IList<T>> massage = null, bool fixedSort = false, Func<IDbTransaction> getTransaction = null, string outerSqlTemplate = "{0}")
            : base(getConnection, sql, param, map, massage, fixedSort, getTransaction, outerSqlTemplate)
        { }

        public override IList<T> GetPage(int pageNumber, int pageSize, SortField[] sortFields)
        {
            string format;

            if (!_hasOverallCount || _sql.IndexOf("OverallCount", StringComparison.InvariantCultureIgnoreCase) != -1)
                format = "{0} " + (_fixedSort ? "" : "ORDER BY {1}") + " OFFSET {2} ROWS FETCH NEXT {3} ROWS ONLY";
            else
                // TODO: use dapper multimap Query<T, Dictionary<string, object>> to map all summary values in one go
                format = @"
;WITH _data AS (
    {0}
),
    _count AS (
        SELECT COUNT(0) AS OverallCount FROM _data
)
SELECT * FROM _data CROSS APPLY _count " + (_fixedSort ? "" : "ORDER BY {1}") + " OFFSET {2} ROWS FETCH NEXT {3} ROWS ONLY";

            string sql = string.Format(_outerSqlTemplate,
                string.Format(format, _sql, BuildSortClause(sortFields) ?? "CURRENT_TIMESTAMP", pageNumber * pageSize, pageSize));

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
