using Griddly.Models;
using Griddly.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
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
            return new GriddlyResult<TestGridItem>(new[]
            {
                new TestGridItem() { Field1 = "xyzzy", Field2 = "plugh"},
                new TestGridItem() { Field1 = "y2", Field2 = "cabin"},
                new TestGridItem() { Field1 = "y2", Field2 = "cabin"},
                new TestGridItem() { Field1 = "y2", Field2 = "cabin"},
                new TestGridItem() { Field1 = "y2", Field2 = "cabin"},
                new TestGridItem() { Field1 = "y2", Field2 = "cabin"},
                new TestGridItem() { Field1 = "y2", Field2 = "cabin"},
                new TestGridItem() { Field1 = "y2", Field2 = "cabin"},
                new TestGridItem() { Field1 = "y2", Field2 = "cabin"},
                new TestGridItem() { Field1 = "y2", Field2 = "cabin"},
                new TestGridItem() { Field1 = "y2", Field2 = "cabin"},
                new TestGridItem() { Field1 = "y2", Field2 = "cabin"},
                new TestGridItem() { Field1 = "y2", Field2 = "cabin"},
                new TestGridItem() { Field1 = "y2", Field2 = "cabin"},
                new TestGridItem() { Field1 = "y2", Field2 = "cabin"},
                new TestGridItem() { Field1 = "y2", Field2 = "cabin"},
                new TestGridItem() { Field1 = "y2", Field2 = "cabin"},
                new TestGridItem() { Field1 = "y2", Field2 = "cabin"},
                new TestGridItem() { Field1 = "y2", Field2 = "cabin"},
                new TestGridItem() { Field1 = "y2", Field2 = "cabin"},
                new TestGridItem() { Field1 = "y2", Field2 = "cabin"},
                new TestGridItem() { Field1 = "y2", Field2 = "cabin"},
                new TestGridItem() { Field1 = "y2", Field2 = "cabin"},
                new TestGridItem() { Field1 = "y2", Field2 = "cabin"},
                new TestGridItem() { Field1 = "y2", Field2 = "cabin"},
                new TestGridItem() { Field1 = "y2", Field2 = "cabin"},
                new TestGridItem() { Field1 = "y2", Field2 = "cabin"},
                new TestGridItem() { Field1 = "y2", Field2 = "cabin"},
                new TestGridItem() { Field1 = "y2", Field2 = "cabin"},
            }.AsQueryable());
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
    }
}