using System;
using System.Threading.Tasks;

#if NETCOREAPP
using Microsoft.AspNetCore.Html;
#endif

namespace Griddly.Mvc
{
#if NETCOREAPP
    public class GriddlyHtmlButton : GriddlyButton
    {
        public GriddlyHtmlButton(Func<object, IHtmlContent> content) 
        {
            ContentTask = o => Task.FromResult(content(o));
        }
        public GriddlyHtmlButton(Task<IHtmlContent> content) 
        {
            ContentTask = o => content;
        }

        /// <summary>
        /// Pass the HtmlTemplate in to the constructor instead of using this setter.
        /// Kept for backward compatibility only.
        /// </summary>
        [Obsolete("Pass the HtmlTemplate in to the constructor")]
        public Func<object, IHtmlContent> HtmlTemplate
        {
            set
            {
                var func = value;
                ContentTask = o => Task.FromResult(func(o));
            }
        }

        public Func<object, Task<IHtmlContent>> ContentTask { get; private set; }
    }
#else
    public class GriddlyHtmlButton : GriddlyButton
    {
        public Func<object, object> HtmlTemplate { get; set; }
    }
#endif
}