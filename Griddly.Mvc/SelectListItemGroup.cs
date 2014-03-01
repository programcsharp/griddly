using System.Collections.Generic;
using System.Web.Mvc;

namespace Griddly.Mvc
{
    public class SelectListItemGroup : SelectListItem
    {
        public List<SelectListItem> Items { get; set; }
    }
}
