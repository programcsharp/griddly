using Faker;
using Griddly.Models;
using Griddly.Mvc;
using Griddly.Mvc.Results;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace Griddly.Controllers
{
    public class HomeController : Controller
    {
        public static ActionResult HandleCustomExport(GriddlyResult result, NameValueCollection form, ActionContext context)
        {
            return new JsonResult(result.GetAllForProperty<long?>("Id", null));
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult HistoryTest()
        {
            return View();
        }

        public IActionResult MultipleDefaultTest()
        {
            return View();
        }

        public GriddlyResult Test1Grid(string lastName, string city = "fooorrr")
        {
            this.SetGriddlyDefault(ref lastName, "lastName", "ba");

            IQueryable<TestGridItem> query = _testData;

            if (!string.IsNullOrWhiteSpace(lastName))
                query = query.Where(x => x.LastName.Contains(lastName));
            if (!string.IsNullOrWhiteSpace(city))
                query = query.Where(x => x.City.Contains(city));

            return this.GriddlyQueryable(query);
        }

        public GriddlyResult Test2Grid(string lastName, string city)
        {
            this.SetGriddlyDefault(ref lastName, "city", "fo");

            IQueryable<TestGridItem> query = _testData;

            if (!string.IsNullOrWhiteSpace(lastName))
                query = query.Where(x => x.LastName.Contains(lastName));

            if (!string.IsNullOrWhiteSpace(city))
                query = query.Where(x => x.City.Contains(city));

            return this.GriddlyQueryable(query);
        }

        public IActionResult FilterBar()
        {
            //this.ForceGriddlyDefault("test", "hello world!");

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

            return this.GriddlyQueryable(query);
        }

        public GriddlyResult FilterBoxGrid(string lastName, DateTime? city, string[] item)
        {
            this.SetGriddlyDefault(ref lastName, "lastName", "ba");
            this.SetGriddlyDefault(ref item, "item", new[] { "Item6", "Item7", "Item8", "Item9", "Item10" });

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

            return this.GriddlyQueryable(query);
        }

        public GriddlyResult FilterRangeGrid(DateTime? stateStart, int[] company)
        {
            this.SetGriddlyDefault(ref company, "company", new[] { (int)FilterDataType.String });
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

            return this.GriddlyQueryable(query);
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

            return this.GriddlyQueryable(query);
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Examples()
        {
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
                    Id = (long)i,
                    FirstName = Name.First(),
                    LastName = Name.Last(),
                    Company = Company.Name(),
                    Address = r.Next(short.MaxValue) + " " + Address.StreetName(),
                    City = Address.City(),
                    State = Address.UsStateAbbr(),
                    PostalCode = Address.ZipCode(),
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

        public IActionResult IndexGrid(string item, int? quantityStart, int? quantityEnd, decimal? totalStart, decimal? totalEnd, string firstName, string lastName, bool? isApproved)
        {
            this.SetGriddlyDefault(ref isApproved, "isApproved", false);

            IQueryable<SimpleOrder> query = _indexTestData;

            if (!string.IsNullOrWhiteSpace(item))
                query = query.Where(x => x.Item.ToLower().Contains(item.ToLower()));

            if (quantityStart != null && quantityEnd != null)
                query = query.Where(x => x.Quantity >= quantityStart && x.Quantity <= quantityEnd);
            if (quantityStart != null)
                query = query.Where(x => x.Quantity >= quantityStart);
            if (quantityEnd != null)
                query = query.Where(x => x.Quantity <= quantityEnd);

            if (totalStart != null && totalEnd != null)
                query = query.Where(x => x.Total >= totalStart && x.Total <= totalEnd);
            if (totalStart != null)
                query = query.Where(x => x.Total >= totalStart);
            if (totalEnd != null)
                query = query.Where(x => x.Total <= totalEnd);

            if (!string.IsNullOrWhiteSpace(firstName))
                query = query.Where(x => x.Person.FirstName.ToLower().Contains(firstName.ToLower()));
            if (!string.IsNullOrWhiteSpace(lastName))
                query = query.Where(x => x.Person.LastName.ToLower().Contains(lastName.ToLower()));

            if (isApproved != null)
                query = query.Where(x => x.IsApproved == isApproved);

            return this.GriddlyQueryable(query);
        }

        static readonly IQueryable<SimpleOrder> _indexTestData = BuildIndexTestData().ToList().AsQueryable();

        static IEnumerable<SimpleOrder> BuildIndexTestData()
        {
            List<SimpleOrder> items = new List<SimpleOrder>();

            Random r = new Random();

            int count = r.Next(10000);

            for (int i = 0; i < count; i++)
            {
                yield return new SimpleOrder()
                {
                    Id = i,
                    Item = Lorem.GetFirstWord(),
                    Quantity = 1 + r.Next(10),
                    Total = 1 + (decimal)(r.NextDouble() * 10000),
                    IsApproved = r.Next(10) > 3,
                    Person = new SimplePerson()
                    {
                        Id = r.Next(10000),
                        FirstName = Name.First(),
                        LastName = Name.Last(),
                    }
                };
            }
        }
    }
}