namespace Griddly.Mvc.Exceptions;

public class DapperGriddlyException : Exception
{
    public string Sql { get; protected set; }
    public object Params { get; protected set; }

    public DapperGriddlyException(string message, string sql, object param = null, Exception ex = null)
        : base(message, ex)
    {
        Sql = sql;
        Params = param;
    }
}
