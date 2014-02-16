using Faker;
using Griddly.Models;
using Griddly.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Griddly.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public GriddlyResult TestGrid()
        {
            return new GriddlyResult<TestGridItem>(_testData);
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        static readonly IQueryable<TestGridItem> _testData = BuildTestData().AsQueryable();

        static List<TestGridItem> BuildTestData()
        {
            List<TestGridItem> items = new List<TestGridItem>();
            Random r = new Random();

            for (int i = 0; i < 1000; i++)
            {
                items.Add(new TestGridItem()
                {
                    FirstName = Name.GetFirstName(),
                    LastName = Name.GetLastName(),
                    Company = Company.GetName(),
                    Address = r.Next(short.MaxValue) + " " + Address.GetStreetName(),
                    City = Address.GetCity(),
                    State = Address.GetUSState(),
                    PostalCode = Address.GetZipCode(),
                });
            }

            return items;
        }
    }
}