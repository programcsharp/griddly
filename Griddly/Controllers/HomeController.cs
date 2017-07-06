using Faker;
using Griddly.Models;
using Griddly.Mvc;
using Griddly.Mvc.Results;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web.Mvc;

namespace Griddly.Controllers
{
    public class HomeController : Controller
    {
        public static ActionResult HandleCustomExport(GriddlyResult result, NameValueCollection form)
        {
            return new JsonResult()
            {
                Data = result.GetAllForProperty<long?>("Id", null),
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
            };
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult HistoryTest()
        {
            return View();
        }


        public ActionResult Editly()
        {
            return View();
        }

        public ActionResult EditlyGrid(string item, int? quantityStart, int? quantityEnd, decimal? totalStart, decimal? totalEnd, string firstName, string lastName, bool? isApproved)
        {
            this.SetGriddlyDefault(ref isApproved, "isApproved", true);

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

            return new QueryableResult<SimpleOrder>(query);
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

            return new QueryableResult<TestGridItem>(query);
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

            return new QueryableResult<TestGridItem>(query);
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

            return new QueryableResult<TestGridItem>(query);
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

            return new QueryableResult<TestGridItem>(query);
        }

        public ActionResult About()
        {
            return View();
        }

        public ActionResult Examples()
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
                    FirstName = Name.GetFirstName(),
                    LastName = Name.GetLastName(),
                    Company = Company.GetName(),
                    Address = r.Next(short.MaxValue) + " " + Address.GetStreetName(),
                    City = Address.GetCity(),
                    State = Address.GetUSState(),
                    PostalCode = Address.GetZipCode(),
                    Test = (decimal)(r.NextDouble() * 10000),
                    NullThing = (string)null,
                });
            }

            return items;
        }

        public ActionResult IndexGrid(string item, int? quantityStart, int? quantityEnd, decimal? totalStart, decimal? totalEnd, string firstName, string lastName, bool? isApproved)
        {
            this.SetGriddlyDefault(ref isApproved, "isApproved", true);

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

            return new QueryableResult<SimpleOrder>(query);
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
                    Item = Lorem.GetWord(),
                    Quantity = 1 + r.Next(10),
                    Total = 1 + (decimal)(r.NextDouble() * 10000),
                    IsApproved = r.Next(10) > 3,
                    Person = new SimplePerson()
                    {
                        Id = r.Next(10000),
                        FirstName = Name.GetFirstName(),
                        LastName = Name.GetLastName(),
                    }
                };
            }
        }
    }
}