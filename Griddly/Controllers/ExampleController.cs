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
        public ActionResult Filters()
        {
            return View("Example", new ExampleModel()
            {
                Title = "Filters Example",
                GridAction = "FiltersGrid",
                GridView = "FiltersGrid.cshtml",
                Description = "There are several filter helpers built into Griddly. Click the \"Filter\" button to play with these."
            });
        }

        #region Test Data

        static readonly IQueryable<TestGridItem> _testData = BuildTestData().AsQueryable();

        static List<TestGridItem> BuildTestData()
        {
            List<TestGridItem> items = new List<TestGridItem>();
            Random r = new Random();

            for (int i = 0; i < 1000; i++)
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