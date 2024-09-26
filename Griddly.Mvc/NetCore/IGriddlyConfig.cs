using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Griddly.Mvc;

public interface IGriddlyConfig : IGriddlyColumnValueFilter
{
    bool IsCookiesDisabled();

    Task OnBeforeRender(BeforeRenderArgs e);
    Task OnGriddlyResultExecuting(GriddlyResultExecutingArgs e);
    Task OnGriddlyPageExecuting(GriddlyPageExecutingArgs e);
    Task<IEnumerable> OnGriddlyExportExecuting(IEnumerable items, GriddlySettings griddlySettings);
    Task<ActionResult> HandleCustomExport(HandleCustomExportArgs e);

    Func<GriddlyButton, IHtmlHelper, Task<IHtmlContent>> IconTemplate { get; }
    Func<GriddlyResultPage, IHtmlHelper, Task<IHtmlContent>> FooterTemplate { get; }
    Func<GriddlyResultPage, IHtmlHelper, Task<IHtmlContent>> HeaderTemplate { get; }
    Func<IGriddlyFilterSettings, IHtmlHelper, Task<IHtmlContent>> FilterModalHeaderTemplate { get; }
    Func<IGriddlyFilterSettings, IHtmlHelper, Task<IHtmlContent>> FilterModalFooterTemplate { get; }
    Func<EmptyGridMessageTemplateParams, Task<IHtmlContent>> EmptyGridMessageTemplate { get; }
}
