namespace Griddly.Mvc;

//TODO: Put GriddlySettings into GriddlyContext so that it doesn't need to be passed in separately
public class EmptyGridMessageTemplateParams
{
    public GriddlyResultPage ResultPage { get; set; }
    public GriddlySettings Settings { get; set; }
}
