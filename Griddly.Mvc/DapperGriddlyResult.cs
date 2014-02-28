using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Helpers;

namespace Griddly.Mvc
{
    public class DapperGriddlyResult<T> : GriddlyResult<T>
    {
        Func<IDbConnection> _getConnection;
        string _sql;
        object _param;
        Func<IDbConnection, string, object, IEnumerable<T>> _map;
        Action<IDbConnection, IList<T>> _massage;

        long? _overallCount = null;
        bool _fixedSort;

        public DapperGriddlyResult(Func<IDbConnection> getConnection, string sql, object param, Func<IDbConnection, string, object, IEnumerable<T>> map = null, Action<IDbConnection, IList<T>> massage = null, bool fixedSort = false)
            : base(null)
        {
            _getConnection = getConnection;
            _sql = sql;
            _param = param;

            if (map == null)
                _map = DefaultMap;
            else
                _map = map;

            _massage = massage;
            _fixedSort = fixedSort;
        }

        public override long GetCount()
        {
            if (_overallCount == null)
            {
                string sql = string.Format("SELECT CAST(COUNT(*) as bigint) FROM ({0}) [_proj]", _sql);

                IDbConnection cn = _getConnection();

                _overallCount = cn.Query<long>(sql, _param).Single();
            }

            return _overallCount.Value;
        }

        public override IList<T> GetPage(int pageNumber, int pageSize, SortField[] sortFields)
        {
            string sql = string.Format("{0} " + (_fixedSort ? "" : "ORDER BY {1}") + " OFFSET {2} ROWS FETCH NEXT {3} ROWS ONLY", _sql, BuildSortClause(sortFields), pageNumber * pageSize, pageSize);

            return ExecuteQuery(sql, _param);
        }

        public override IEnumerable<T> GetAll(SortField[] sortFields)
        {
            string sql = _fixedSort ? _sql : string.Format("{0} ORDER BY {1}", _sql, BuildSortClause(sortFields));

            return ExecuteQuery(sql, _param);
        }

        protected string BuildSortClause(SortField[] sortFields)
        {
            if (sortFields != null && sortFields.Length > 0)
                return string.Join(",", sortFields.Select(x => x.Field + " " + (x.Direction == SortDirection.Ascending ? "ASC" : "DESC")));
            else
                return "CURRENT_TIMESTAMP";
        }

        // TODO: return IEnumerable so we don't have to .ToList()
        IList<T> ExecuteQuery(string sql, object param)
        {
            IEnumerable<T> result = _map(_getConnection(), sql, param);
            IHasOverallCount overallCount = result as IHasOverallCount;

            if (overallCount != null)
                _overallCount = overallCount.OverallCount;

            IList<T> results = result.ToList();

            if (_massage != null)
                _massage(_getConnection(), results);

            return results;
        }

        protected IEnumerable<T> DefaultMap(IDbConnection cn, string sql, object param)
        {
            IEnumerable<T> result = cn.Query<T>(sql, param);

            if(typeof(IHasOverallCount).IsAssignableFrom(typeof(T))){
                IHasOverallCount firstRow = result.FirstOrDefault() as IHasOverallCount;
                if (firstRow != null)
                {
                    var lp = new ListPage<T>();
                    lp.OverallCount = firstRow.OverallCount;
                    lp.AddRange(result);
                    result = lp;
                }
                else
                {
                    return new ListPage<T>();
                }
            }
            return result;

        }
    }
}
