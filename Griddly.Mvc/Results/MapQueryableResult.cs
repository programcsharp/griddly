using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
#if NET45_OR_GREATER
using Griddly.Mvc.Linq.Dynamic;
using System.Web.Helpers;
using System.Web.Mvc;
#else
using Microsoft.AspNetCore.Mvc.ViewFeatures;
#endif

namespace Griddly.Mvc.Results
{
    public class MapQueryableResult<TIn, TOut> : GriddlyResult<TOut>
    {
        QueryableResult<TIn> _result;

        Func<IEnumerable<TIn>, IEnumerable<TOut>> _map = null;

        public MapQueryableResult(IQueryable<TIn> result,
#if !NET45_OR_GREATER
            ViewDataDictionary viewData,
#endif
            Func<IEnumerable<TIn>, IEnumerable<TOut>> map, string viewName = null, Func<IQueryable<TIn>, IQueryable<TIn>> massage = null) : base(
#if !NET45_OR_GREATER
                viewData,
#endif
                viewName)
        {
            _result = new QueryableResult<TIn>(result,
#if !NET45_OR_GREATER
                viewData,
#endif
                massage: massage);
            _map = map;
        }

        public override IEnumerable<TOut> GetAll(SortField[] sortFields)
        {
            return _map(_result.GetAll(sortFields));
        }

        public override IList<TOut> GetPage(int pageNumber, int pageSize, SortField[] sortFields)
        {
            return _map(_result.GetPage(pageNumber, pageSize, sortFields)).ToList();
        }

        public override void PopulateSummaryValues(GriddlySettings<TOut> settings)
        {
            foreach (GriddlyColumn c in settings.Columns.Where(x => x.SummaryFunction != null))
                _result.PopulateSummaryValue(c);
        }

        public override IEnumerable<P> GetAllForProperty<P>(string propertyName, SortField[] sortFields)
        {
            return _result.GetAllForProperty<P>(propertyName, sortFields);
        }

        public override long GetCount()
        {
            return _result.GetCount();
        }
    }
}
