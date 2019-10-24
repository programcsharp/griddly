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
        public GriddlyResult FilterDefaultsGrid(string[] item, string lastName, bool? isApproved)
        {
            this.SetGriddlyDefault(ref isApproved, nameof(isApproved), true);

            this.SetGriddlyDefault(ref item, nameof(item), new[] { "Item1", "Item2" });

            IQueryable<TestGridItem> query = _testData.AsQueryable();

            if (item != null && item.Any())
                query = query.Where(x => item.Contains(x.Item));

            if (!string.IsNullOrWhiteSpace(lastName))
                query = query.Where(x => x.LastName.IndexOf(lastName, StringComparison.InvariantCultureIgnoreCase) > -1);

            if (isApproved != null)
                query = query.Where(x => x.IsApproved == isApproved.Value);

            return new QueryableResult<TestGridItem>(query);
        }
    }
}