using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Griddly.Mvc
{
    public class GriddlyButton
    {
        public GriddlyButton(string additionalClassName = null)
        {
            Buttons = new List<GriddlyButton>();

            Enabled = true;
            Action = GriddlyButtonAction.Navigate;
            ClearSelectionOnAction = true;

            ClassName = ((GriddlySettings.DefaultButtonClassName ?? "") + " " + (additionalClassName ?? "")).Trim();
        }

        public string Argument { get; set; }
        public Func<object, object> ArgumentTemplate { get; set; }

        public bool Enabled { get; set; }
        public bool EnableOnSelection { get; set; }
        public bool IsSeparator { get; set; }
        public bool IsSplitDropdown { get; set; }

        /// <summary>
        /// Clear the current row selections after this button is activated (default: true)
        /// </summary>
        public bool ClearSelectionOnAction { get; set; }

        public string Text { get; set; }
        public string Title { get; set; }
        public string Icon { get; set; }
        public string ClassName { get; set; }
        public string Target { get; set; }
        public string ConfirmMessage { get; set; }
        public bool AlignRight { get; set; }
        
        /// <summary>
        /// The row ids to include in the button action (default uses grid default)
        /// </summary>
        public string[] RowIds { get; set; }

        /// <summary>
        /// Append the selected row ids to the button href as comma separated query strings (only navigate and modal types supported)
        /// </summary>
        public bool AppendRowIdsToUrl { get; set; }

        public IDictionary<string, object> HtmlAttributes { get; set; }

        public GriddlyButtonAction Action { get; set; }

        public List<GriddlyButton> Buttons { get; set; }

        public GriddlyButton Add(GriddlyButton item)
        {
            Buttons.Add(item);
            return this;
        }

        public HttpVerbs Verb
        {
            get
            {
                switch (Action)
                {
                    case GriddlyButtonAction.Navigate:
                        return HttpVerbs.Get;
                    case GriddlyButtonAction.Post:
                    case GriddlyButtonAction.PostCriteria:
                    case GriddlyButtonAction.Ajax:
                    case GriddlyButtonAction.AjaxBulk:
                        return HttpVerbs.Post;
                    default:
                        return HttpVerbs.Get;
                }

            }
        }
    }

    public enum GriddlyButtonAction
    {
        Navigate = 1,
        Javascript,
        Ajax,
        AjaxBulk,
        Post,
        Modal,
        PostCriteria
    }
}
