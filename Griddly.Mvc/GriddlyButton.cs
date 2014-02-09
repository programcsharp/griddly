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
        public bool? EnableOnSelection { get; set; }
        public bool IsSeparator { get; set; }
        public string Text { get; set; }
        public string Icon { get; set; }
        public string ClassName { get; set; }
        public string Target { get; set; }

        GriddlyButtonAction _action;

        public GriddlyButtonAction Action
        {
            get { return _action; }
            set
            {
                if ((value == GriddlyButtonAction.Ajax || value == GriddlyButtonAction.AjaxBulk || value == GriddlyButtonAction.Post) && EnableOnSelection == null)
                    EnableOnSelection = true;

                _action = value;
            }
        }

        public List<GriddlyButton> Buttons { get; private set; }

        public GriddlyButton()
        {
            Buttons = new List<GriddlyButton>();

            Enabled = true;
            Action = GriddlyButtonAction.Navigate;
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
        Report
    }
}
