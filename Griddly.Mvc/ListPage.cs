namespace Griddly.Mvc;

public class ListPage<T> : List<T>, IHasOverallCount
{
    public long OverallCount { get; set; }
}
