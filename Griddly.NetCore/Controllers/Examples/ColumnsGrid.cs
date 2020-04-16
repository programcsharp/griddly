using Griddly.Models;
using Griddly.Mvc;
using Griddly.Mvc.Results;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace Griddly.Controllers
{
    public partial class ExampleController : Controller
    {
        public GriddlyResult ColumnsGrid()
        {
            IQueryable<TestGridItem> query = _testData.AsQueryable();

            return new QueryableResult<TestGridItem>(query);
        }
    }
}