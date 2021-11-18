using System.Collections.Generic;
#if NETFRAMEWORK
using System.Web.Mvc;
#else
using Microsoft.AspNetCore.Mvc.Rendering;
#endif


namespace Griddly.Mvc
{
    public class SelectListItemGroup : SelectListItem
    {
        public List<SelectListItem> Items { get; set; }
    }
}
