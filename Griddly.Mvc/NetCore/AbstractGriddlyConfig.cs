using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Griddly.Mvc;

public abstract class AbstractGriddlyConfig : IGriddlyConfig
{
    private readonly IHttpContextAccessor accessor;
    protected HttpContext HttpContext => accessor?.HttpContext;

    public AbstractGriddlyConfig(IHttpContextAccessor accessor)
    {
        this.accessor = accessor;
    }
    
    public virtual bool IsCookiesDisabled()
    {
        return GriddlySettings.IsCookiesDisabledDefault;
    }

    #region Event handlers
    public virtual Task<IEnumerable> OnGriddlyExportExecuting(IEnumerable items, GriddlySettings griddlySettings)
    {
        return Task.FromResult(items);
    }

    public virtual Task<ActionResult> HandleCustomExport(HandleCustomExportArgs e)
    {
        throw new NotImplementedException("Please implement HandleCustomExport in your GriddlyConfig class");
    }

    public virtual Task OnBeforeRender(BeforeRenderArgs e)
    {
        return Task.CompletedTask;
    }

    public virtual Task OnGriddlyResultExecuting(GriddlyResultExecutingArgs e)
    {
        return Task.CompletedTask;
    }

    public virtual Task OnGriddlyPageExecuting(GriddlyPageExecutingArgs e)
    {
        return Task.CompletedTask;
    }
    #endregion

    #region Html Templates
    public Func<GriddlyButton, IHtmlHelper, Task<IHtmlContent>> IconTemplate { get; protected set; }
    public Func<GriddlyResultPage, IHtmlHelper, Task<IHtmlContent>> FooterTemplate { get; protected set; }
    public Func<GriddlyResultPage, IHtmlHelper, Task<IHtmlContent>> HeaderTemplate { get; protected set; }
    public Func<IGriddlyFilterSettings, IHtmlHelper, Task<IHtmlContent>> FilterModalHeaderTemplate { get; protected set; }
    public Func<IGriddlyFilterSettings, IHtmlHelper, Task<IHtmlContent>> FilterModalFooterTemplate { get; protected set; }
    public Func<EmptyGridMessageTemplateParams, Task<IHtmlContent>> EmptyGridMessageTemplate { get; protected set; }
    #endregion

    #region IGriddlyColumnValueFilter implementation
    public virtual object FilterColumnValue(GriddlyColumn column, object value, HttpContext httpContext)
    {
        return value;
    }

    object IGriddlyColumnValueFilter.Filter(GriddlyColumn column, object value, HttpContext httpContext)
    {
        return FilterColumnValue(column, value, httpContext);
    }
    #endregion
}