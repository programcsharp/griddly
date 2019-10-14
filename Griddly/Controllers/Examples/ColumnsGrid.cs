using Griddly.Models;
using Griddly.Mvc;
using Griddly.Mvc.Results;
using System.Linq;
using System.Web.Mvc;

namespace Griddly.Controllers
{
    public partial class ExampleController : Controller
    {
        public GriddlyResult ColumnsGrid()
        {
            IQueryable<TestGridItem> query = _testData;

            return new QueryableResult<TestGridItem>(query);
        }
    }
}