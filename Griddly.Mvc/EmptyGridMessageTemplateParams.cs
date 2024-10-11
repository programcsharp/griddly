namespace Griddly.Mvc;

//TODO: Put GriddlySettings into GriddlyContext so that it doesn't need to be passed in separately
public class EmptyGridMessageTemplateParams
{
    public EmptyGridMessageTemplateParams(GriddlyResultPage resultPage, GriddlySettings settings, IHtmlHelper html)
    {
        Html = html;
        ResultPage = resultPage;
        Settings = settings;
    }

    public IHtmlHelper Html { get; set; }
    public GriddlyResultPage ResultPage { get; set; }
    public GriddlySettings Settings { get; set; }
}
