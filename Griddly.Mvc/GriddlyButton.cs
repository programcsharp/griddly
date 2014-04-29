using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Griddly.Mvc
{
    public class GriddlyButton
    {
        public string Argument { get; set; }
        public Func<object, object> ArgumentTemplate { get; set; }

        public bool Enabled { get; set; }
        public bool EnableOnSelection { get; set; }
        public bool IsSeparator { get; set; }
        public bool IsSplitDropdown { get; set; }
        public string Text { get; set; }
        public string Icon { get; set; }
        public string ClassName { get; set; }
        public string Target { get; set; }
        public bool AlignRight { get; set; }

        public GriddlyButtonAction Action { get; set; }

        public List<GriddlyButton> Buttons { get; set; }

        public GriddlyButton()
        {
            Buttons = new List<GriddlyButton>();

            Enabled = true;
            Action = GriddlyButtonAction.Navigate;
            ClassName = GriddlySettings.DefaultButtonClassName;
        }

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
                    case GriddlyButtonAction.Report:
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
        Report,
        PostCriteria
    }
}
