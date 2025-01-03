using Dapper;
using Griddly.Mvc.Exceptions;
using System.Data;
using System.Text;

namespace Griddly.Mvc.Results;

public abstract class DapperResult<T> : GriddlyResult<T>
{
    Func<IDbConnection> _getConnection;
    Func<IDbTransaction> _getTransaction;
    object _param;
    Func<IDbConnection, IDbTransaction, string, object, IEnumerable<T>> _map;
    Action<IDbConnection, IDbTransaction, IList<T>> _massage;
    long? _overallCount = null;
    int? _commandTimeout = null;

    protected string _outerSqlTemplate;
    protected string _sql;
    protected bool _fixedSort;
    protected static readonly bool _hasOverallCount = typeof(IHasOverallCount).IsAssignableFrom(typeof(T));

    public DapperResult(Func<IDbConnection> getConnection, string sql, object param, 
#if NETCOREAPP
        ViewDataDictionary viewData,
#endif
        Func<IDbConnection, IDbTransaction, string, object, IEnumerable<T>> map, Action<IDbConnection, IDbTransaction, IList<T>> massage, bool fixedSort, Func<IDbTransaction> getTransaction, string outerSqlTemplate, int? commandTimout = null)
        : base(
#if NETCOREAPP
              viewData,
#endif
              null)
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
        _commandTimeout = commandTimout;
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

    public override IEnumerable<P> GetAllForProperty<P>(string propertyName, SortField[] sortFields, P[] restriction = null)
    {
        if (propertyName.Contains("."))
            throw new ArgumentException($"Property name may not contain a period. \"{propertyName}\"", "propertyName");

        string sql = @$"SELECT {propertyName} as _val 
FROM ({_sql}) [_proj]
{(restriction != null ? $"WHERE {propertyName} in @GetAllForProperty_Ids" : null)}
{(_fixedSort ? "" : $"ORDER BY {BuildSortClause(sortFields) ?? "CURRENT_TIMESTAMP"}")}";

        try
        {
            IDbConnection cn = _getConnection();
            IDbTransaction tx = _getTransaction != null ? _getTransaction() : null;

            object parameters = _param;

            if (restriction != null)
            {
                var parameters2 = new DynamicParameters(parameters);
                parameters2.Add("GetAllForProperty_Ids", restriction);
                parameters = parameters2;
            }

            return cn.Query<P>(sql, parameters, tx, commandTimeout: _commandTimeout);
        }
        catch (Exception ex)
        {
            throw new DapperGriddlyException($"Error selecting property: {propertyName}.", sql, _param, ex);
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

        return cn.Query<X>(sql, _param, tx, commandTimeout: _commandTimeout).Single();
    }

    // TODO: return IEnumerable so we don't have to .ToList()
    protected virtual IList<T> ExecuteQuery(string sql)
    {
        try
        {
            IEnumerable<T> result = _map(_getConnection(), _getTransaction != null ? _getTransaction() : null, sql, _param);
            
            if (result is IHasOverallCount)
                _overallCount = (result as IHasOverallCount).OverallCount;

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
        IEnumerable<T> result = cn.Query<T>(sql, param, tx, commandTimeout: _commandTimeout);

        var firstRow = result.FirstOrDefault();
        long? overallCount = null;

        if (firstRow == null)
        {
            overallCount = 0;
        }
        else if (_hasOverallCount && firstRow is IHasOverallCount)
        {
            overallCount = (firstRow as IHasOverallCount).OverallCount;
        }
        else if (_sql.IndexOf("OverallCount", StringComparison.InvariantCultureIgnoreCase) != -1 && firstRow is IDictionary<string, object>)
        {
            overallCount = Convert.ToInt64((firstRow as IDictionary<string, object>)["OverallCount"]);
        }

        if (overallCount != null)
        {
            ListPage<T> lp = new ListPage<T>();

            lp.OverallCount = overallCount.Value;
            lp.AddRange(result);

            result = lp;
        }

        return result;
    }
}
