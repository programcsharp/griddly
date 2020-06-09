using Griddly.Models;
using Griddly.Mvc;
using Griddly.Mvc.Results;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace Griddly.Controllers
{
    public partial class ExampleController : Controller
    {
        public GriddlyResult ButtonsGrid(string lastName)
        {
            IQueryable<TestGridItem> query = _testData.AsQueryable();

            if (lastName != null)
                query = query.Where(x => x.LastName.IndexOf(lastName, System.StringComparison.InvariantCultureIgnoreCase) > -1);

            return this.GriddlyQueryable(query);
        }

        [HttpPost]
        public IActionResult ButtonsPost(long[] ids)
        {
            ViewBag.Selected = _testData.AsQueryable()
                .Where(x => ids.Any(i => i == x.Id))
                .Select(x => $"{x.FirstName} {x.LastName}")
                .ToArray();

            return Buttons();
        }

        [HttpPost]
        public IActionResult ButtonsPostCriteria(string lastName)
        {
            return Buttons(lastName);
        }

        public IActionResult ButtonsAjax(long id)
        {
            return Json(new 
            {
                selected = _testData.AsQueryable()
                    .Where(x => id == x.Id)
                    .Select(x => $"{x.FirstName} {x.LastName}")
                    .FirstOrDefault()
            });
        }

        public IActionResult ButtonsAjaxBulk(long[] ids)
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