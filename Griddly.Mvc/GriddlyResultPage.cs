using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace Griddly.Mvc
{
    public class GriddlyResultPage
    {
        public IEnumerable Data { get; set; }
        public string[] SortFields { get; set; }

        public int PageNumber { get; set; }
        public int Count { get; set; }
        public long Total { get; set; }

        public int PageSize { get; set; }

        public GriddlySettings Settings { get; set; }
    }

    public class GriddlyResultPage<T> : GriddlyResultPage
    {
        public GriddlyResultPage() { }
        public GriddlyResultPage(IEnumerable<T> data)
        {
            this.Data = data;
            Total = PageSize = Count = data.Count();
        }
        public new IEnumerable<T> Data
        {
            get { return (IEnumerable<T>)base.Data; }
            set { base.Data = value; }
        }
    }
}
