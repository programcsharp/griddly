namespace Griddly.Mvc;

public class GriddlyHtmlButton : GriddlyButton
{
    public Func<object,
#if NETCOREAPP
        IHtmlContent
#else
        object
#endif
        > HtmlTemplate { get; set; }
}