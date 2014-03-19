using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Griddly.Mvc
{
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
}
