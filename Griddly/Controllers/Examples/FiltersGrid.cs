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
        public GriddlyResult FiltersGrid(string[] item, string lastName, string[] state, int? quantityFrom, int? quantityTo, DateTime? dateFrom, DateTime? dateTo, bool? isApproved)
        {
            IQueryable<TestGridItem> query = _testData;

            if (item != null && item.Any())
                query = query.Where(x => item.Contains(x.Item));

            if (!string.IsNullOrWhiteSpace(lastName))
                query = query.Where(x => x.LastName.Contains(lastName));

            if (state != null && state.Any())
                query = query.Where(x => state.Contains(x.State));

            if (quantityFrom != null)
                query = query.Where(x => x.Quantity >= quantityFrom);

            if (quantityTo != null)
                query = query.Where(x => x.Quantity <= quantityTo);

            if (dateFrom != null)
                query = query.Where(x => x.Date >= dateFrom);

            if (dateTo != null)
                query = query.Where(x => x.Date < dateTo.Value.AddDays(1));

            if (isApproved != null)
                query = query.Where(x => x.IsApproved == isApproved.Value);

            return new QueryableResult<TestGridItem>(query);
        }
    }
}