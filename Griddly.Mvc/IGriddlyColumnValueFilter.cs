namespace Griddly.Mvc;

public interface IGriddlyColumnValueFilter
{
    object Filter(GriddlyColumn column, object value, HttpContextBase httpContext);
}
