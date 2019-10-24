using Griddly.Models;
using Griddly.Mvc;
using Griddly.Mvc.Results;
using System;
using System.Linq;
using System.Web.Mvc;

namespace Griddly.Controllers
{
    public partial class ExampleController : Controller
    {
        public GriddlyResult ParametersGrid(string lastname)
        {
            ViewBag.TitleParameter = $"Last Name starts with \"{lastname}\"";

            IQueryable<TestGridItem> query = _testData.AsQueryable();

            if (!string.IsNullOrWhiteSpace(lastname))
                query = query.Where(x => x.LastName.StartsWith(lastname, StringComparison.InvariantCultureIgnoreCase));

            return new QueryableResult<TestGridItem>(query);
        }
    }
}