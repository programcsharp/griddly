using Griddly.Models;
using Griddly.Mvc;
using Griddly.Mvc.Results;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace Griddly.Controllers
{
    public partial class ExampleController : Controller
    {
        IWebHostEnvironment _env;
        public ExampleController(IWebHostEnvironment env)
        {
            _env = env;
        }

        public GriddlyResult ParametersGrid(string lastname, long?[] collection)
        {
            ViewBag.TitleParameter = $"Last Name starts with \"{lastname}\"";

            IQueryable<TestGridItem> query = _testData.AsQueryable();

            if (!string.IsNullOrWhiteSpace(lastname))
                query = query.Where(x => x.LastName.StartsWith(lastname, StringComparison.InvariantCultureIgnoreCase));

            return this.GriddlyQueryable(query);
        }
    }
}