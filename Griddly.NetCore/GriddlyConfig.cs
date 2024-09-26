using Griddly.Mvc;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Threading.Tasks;

namespace Griddly.NetCore
{
    public class GriddlyConfig : AbstractGriddlyConfig
    {
        public GriddlyConfig(IHttpContextAccessor accessor) : base(accessor)
        {
        }

        public override Task<ActionResult> HandleCustomExport(HandleCustomExportArgs e)
        {
            return Task.FromResult<ActionResult>(
                new JsonResult(e.Result.GetAllForProperty<long?>("Id", null))
            );
        }
    }
}