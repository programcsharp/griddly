using Faker;
using Griddly.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Griddly.Controllers
{
    public partial class ExampleController : Controller
    {
        public ActionResult Columns()
        {
            return View("Example", new ExampleModel()
            {
                Title = "Columns Example",
                GridAction = "ColumnsGrid",
                ParentView = "Columns.cshtml",
                GridView = "ColumnsGrid.cshtml",
                Description = "Griddly column helpers offer several ways to quickly define your table structure."
            });
        }

        public ActionResult Filters()
        {
            return View("Example", new ExampleModel()
            {
                Title = "Filters Example",
                GridAction = "FiltersGrid",
                ParentView = "Filters.cshtml",
                GridView = "FiltersGrid.cshtml",
                Description = "There are several filter helpers built into Griddly. Click the \"Filter\" button to play with these."
            });
        }

        public ActionResult FilterDefaults()
        {
            return View("Example", new ExampleModel()
            {
                Title = "Filter Defaults Example",
                GridAction = "FilterDefaultsGrid",
                ParentView = "FilterDefaults.cshtml",
                GridView = "FilterDefaultsGrid.cshtml",
                Description = "Filter default values may be set using the ControllerBase.SetGriddlyDefault() extension method."
            });
        }

        public ActionResult Parameters()
        {
            return View("Example", new ExampleModel()
            {
                Title = "Additional Parameters Example",
                GridAction = "ParametersGrid",
                ParentView = "Parameters.cshtml",
                GridView = "ParametersGrid.cshtml",
                Description = "Non-filter parameters may be passed from the parent view to the grid action on every refresh request."
            });
        }

        #region Test Data

        static readonly IList<TestGridItem> _testData = BuildTestData();

        static List<TestGridItem> BuildTestData(int rows = 1000)
        {
            List<TestGridItem> items = new List<TestGridItem>();
            Random r = new Random();

            for (int i = 0; i < rows; i++)
            {
                items.Add(new TestGridItem()
                {
                    Id = (long)i,
                    FirstName = Name.GetFirstName(),
                    LastName = Name.GetLastName(),
                    Company = Company.GetName(),
                    Address = r.Next(short.MaxValue) + " " + Address.GetStreetName(),
                    City = Address.GetCity(),
                    State = Address.GetUSState(),
                    PostalCode = Address.GetZipCode(),
                    Quantity = 1 + r.Next(10),
                    Total = 1 + (decimal)(r.NextDouble() * 10000),
                    IsApproved = r.Next(10) > 3,
                    Date = GetRandomDate(r, DateTime.Today.AddYears(-10), DateTime.Today.AddYears(10)),
                    Item = "Item" + (1 + r.Next(9)),

                    Test = (decimal)(r.NextDouble() * 10000),
                    NullThing = (string)null,
                });
            }

            return items;
        }

        static DateTime GetRandomDate(Random r, DateTime from, DateTime to)
        {
            var range = to - from;

            var randTimeSpan = new TimeSpan((long)(r.NextDouble() * range.Ticks));

            return from + randTimeSpan;
        }

        #endregion
    }
}