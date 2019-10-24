using Griddly.Models;
using Griddly.Mvc;
using Griddly.Mvc.Results;
using System.Linq;
using System.Web.Mvc;

namespace Griddly.Controllers
{
    public partial class ExampleController : Controller
    {
        public GriddlyResult ButtonsGrid(string lastName)
        {
            IQueryable<TestGridItem> query = _testData.AsQueryable();

            if (lastName != null)
                query = query.Where(x => x.LastName.IndexOf(lastName, System.StringComparison.InvariantCultureIgnoreCase) > -1);

            return new QueryableResult<TestGridItem>(query);
        }

        [HttpPost]
        public ActionResult ButtonsPost(long[] ids)
        {
            ViewBag.Selected = _testData.AsQueryable()
                .Where(x => ids.Any(i => i == x.Id))
                .Select(x => $"{x.FirstName} {x.LastName}")
                .ToArray();

            return Buttons();
        }

        [HttpPost]
        public ActionResult ButtonsPostCriteria(string lastName)
        {
            return Buttons(lastName);
        }

        public ActionResult ButtonsAjax(long id)
        {
            return Json(new 
            {
                selected = _testData.AsQueryable()
                    .Where(x => id == x.Id)
                    .Select(x => $"{x.FirstName} {x.LastName}")
                    .FirstOrDefault()
            });
        }

        public ActionResult ButtonsAjaxBulk(long[] ids)
        {
            return Json(new
            {
                selected = string.Join(", ", _testData.AsQueryable()
                    .Where(x => ids.Any(i => i == x.Id))
                    .Select(x => $"{x.FirstName} {x.LastName}")
                    .ToArray())
            });
        }
    }
}