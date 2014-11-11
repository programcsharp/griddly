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

        public GriddlyResult TestGrid(string firstName, int? zipStart, int? zipEnd)
        {
            IQueryable<TestGridItem> query = _testData;

            if (!string.IsNullOrWhiteSpace(firstName))
                query = query.Where(x => x.FirstName.Contains(firstName));

            if (zipStart != null && zipEnd != null)
                query = query.Where(x => x.PostalCodePrefix >= zipStart.Value && x.PostalCodePrefix <= zipEnd.Value);
            else if (zipStart != null)
                query = query.Where(x => x.PostalCodePrefix >= zipStart.Value);
            else if (zipEnd != null)
                query = query.Where(x => x.PostalCodePrefix <= zipEnd.Value);

            return new GriddlyResult<TestGridItem>(query);
        }

        public GriddlyResult FilterBoxGrid(string lastName, DateTime? city)
        {
            this.SetGriddlyDefault(ref lastName, "lastName", "ba");

            IQueryable<TestGridItem> query = _testData;

            if (!string.IsNullOrWhiteSpace(lastName))
                query = query.Where(x => x.LastName.Contains(lastName));

            //if (!string.IsNullOrWhiteSpace(firstName))
            //    query = query.Where(x => x.FirstName.Contains(firstName));

            //if (zipStart != null && zipEnd != null)
            //    query = query.Where(x => x.PostalCodePrefix >= zipStart.Value && x.PostalCodePrefix <= zipEnd.Value);
            //else if (zipStart != null)
            //    query = query.Where(x => x.PostalCodePrefix >= zipStart.Value);
            //else if (zipEnd != null)
            //    query = query.Where(x => x.PostalCodePrefix <= zipEnd.Value);

            return new GriddlyResult<TestGridItem>(query);
        }

        public GriddlyResult FilterRangeGrid(DateTime? stateStart)
        {
            this.SetGriddlyDefault(ref stateStart, "stateStart", DateTime.Now);

            IQueryable<TestGridItem> query = _testData;

            //if (!string.IsNullOrWhiteSpace(firstName))
            //    query = query.Where(x => x.FirstName.Contains(firstName));

            //if (zipStart != null && zipEnd != null)
            //    query = query.Where(x => x.PostalCodePrefix >= zipStart.Value && x.PostalCodePrefix <= zipEnd.Value);
            //else if (zipStart != null)
            //    query = query.Where(x => x.PostalCodePrefix >= zipStart.Value);
            //else if (zipEnd != null)
            //    query = query.Where(x => x.PostalCodePrefix <= zipEnd.Value);

            return new GriddlyResult<TestGridItem>(query);
        }

        public GriddlyResult FilterListGrid()
        {
            IQueryable<TestGridItem> query = _testData;

            //if (!string.IsNullOrWhiteSpace(firstName))
            //    query = query.Where(x => x.FirstName.Contains(firstName));

            //if (zipStart != null && zipEnd != null)
            //    query = query.Where(x => x.PostalCodePrefix >= zipStart.Value && x.PostalCodePrefix <= zipEnd.Value);
            //else if (zipStart != null)
            //    query = query.Where(x => x.PostalCodePrefix >= zipStart.Value);
            //else if (zipEnd != null)
            //    query = query.Where(x => x.PostalCodePrefix <= zipEnd.Value);

            return new GriddlyResult<TestGridItem>(query);
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
                    Test = (decimal)(r.NextDouble() * 10000)
                });
            }

            return items;
        }
    }
}