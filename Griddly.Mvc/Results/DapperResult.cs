using Dapper;
using Griddly.Mvc.Exceptions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Web.Helpers;

namespace Griddly.Mvc.Results
{
    public abstract class DapperResult<T> : GriddlyResult<T>
    {
        Func<IDbConnection> _getConnection;
        Func<IDbTransaction> _getTransaction;
        object _param;
        Func<IDbConnection, IDbTransaction, string, object, IEnumerable<T>> _map;
        Action<IDbConnection, IDbTransaction, IList<T>> _massage;
        long? _overallCount = null;

        protected string _outerSqlTemplate;
        protected string _sql;
        protected bool _fixedSort;
        protected static readonly bool _hasOverallCount = typeof(IHasOverallCount).IsAssignableFrom(typeof(T));

        public DapperResult(Func<IDbConnection> getConnection, string sql, object param, Func<IDbConnection, IDbTransaction, string, object, IEnumerable<T>> map, Action<IDbConnection, IDbTransaction, IList<T>> massage, bool fixedSort, Func<IDbTransaction> getTransaction, string outerSqlTemplate)
            : base(null)
        {
            _getConnection = getConnection;
            _sql = sql;
            _param = param;
            _outerSqlTemplate = outerSqlTemplate;

            if (map == null)
                _map = DefaultMap;
            else
                _map = map;

            _massage = massage;
            _fixedSort = fixedSort;
            _getTransaction = getTransaction;

        }

        public override void PopulateSummaryValues(GriddlySettings<T> settings)
        {
            List<GriddlyColumn> summaryColumns = settings.Columns.Where(x => x.SummaryFunction != null).ToList();

            if (summaryColumns.Any())
            {
                StringBuilder aggregateExpression = new StringBuilder();

                aggregateExpression.Append("SELECT ");

                for (int i = 0; i < summaryColumns.Count; i++)
                {
                    if (i > 0)
                        aggregateExpression.Append(", ");

                    GriddlyColumn col = summaryColumns[i];

                    aggregateExpression.AppendFormat("{0}({1}) AS _a{2}", col.SummaryFunction, col.ExpressionString, i);
                }

                string sql = string.Format(_outerSqlTemplate,
                    string.Format("{0} FROM ({1}) [_proj]", aggregateExpression.ToString(), _sql));

                try
                {
                    IDictionary<string, object> item = ExecuteSingle<dynamic>(sql);

                    for (int i = 0; i < summaryColumns.Count; i++)
                        summaryColumns[i].SummaryValue = item["_a" + i];
                }
                catch (Exception ex)
                {
                    throw new DapperGriddlyException("Error populating summary values.", sql, _param, ex);
                }
            }
        }

        public override IEnumerable<P> GetAllForProperty<P>(string propertyName)
        {
            string sql = string.Format("SELECT {0} as _val FROM ({1}) [_proj]", propertyName, _sql);

            try
            {
                IDbConnection cn = _getConnection();
                IDbTransaction tx = _getTransaction != null ? _getTransaction() : null;

                return cn.Query<P>(sql, _param, tx);
            }
            catch (Exception ex)
            {
                throw new DapperGriddlyException($"Error selecting property: {propertyName}.", sql, _param, ex: ex);
            }
        }

        public override long GetCount()
        {
            if (_overallCount == null)
            {
                string sql = string.Format(_outerSqlTemplate,
                    string.Format("SELECT CAST(COUNT(*) as bigint) FROM ({0}) [_proj]", _sql));

                try
                {
                    _overallCount = ExecuteSingle<long>(sql);
                }
                catch (Exception ex)
                {
                    throw new DapperGriddlyException("Error executing count query.", sql, _param, ex);
                }
            }

            return _overallCount.Value;
        }
        
        protected string BuildSortClause(SortField[] sortFields)
        {
            if (sortFields != null && sortFields.Length > 0)
                return string.Join(",", sortFields.Select(x => x.Field + " " + (x.Direction == SortDirection.Ascending ? "ASC" : "DESC")));
            else
                return null;
        }

        protected virtual X ExecuteSingle<X>(string sql)
        {
            IDbConnection cn = _getConnection();
            IDbTransaction tx = _getTransaction != null ? _getTransaction() : null;

            return cn.Query<X>(sql, _param, tx).Single();
        }

        // TODO: return IEnumerable so we don't have to .ToList()
        protected virtual IList<T> ExecuteQuery(string sql)
        {
            try
            {
                IEnumerable<T> result = _map(_getConnection(), _getTransaction != null ? _getTransaction() : null, sql, _param);

                if (_hasOverallCount)
                {
                    IHasOverallCount overallCount = result as IHasOverallCount;

                    if (overallCount != null)
                        _overallCount = overallCount.OverallCount;
                }

                IList<T> results = result.ToList();

                if (_massage != null)
                    _massage(_getConnection(), _getTransaction != null ? _getTransaction() : null, results);

                return results;
            }
            catch (Exception ex)
            {
                throw new DapperGriddlyException("Error executing list query.", sql, _param, ex);
            }
        }

        protected IEnumerable<T> DefaultMap(IDbConnection cn, IDbTransaction tx, string sql, object param)
        {
            IEnumerable<T> result = cn.Query<T>(sql, param, tx);

            if (_hasOverallCount)
            {
                IHasOverallCount firstRow = result.FirstOrDefault() as IHasOverallCount;
                ListPage<T> lp = new ListPage<T>();

                if (firstRow != null)
                {
                    lp.OverallCount = firstRow.OverallCount;
                    lp.AddRange(result);
                }

                result = lp;
            }

            return result;
        }
    }
}
