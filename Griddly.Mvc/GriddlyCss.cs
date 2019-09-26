using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Griddly.Mvc
{
    public class GriddlyCssIcons
    {
        public string Calendar { get; set; }
        public string Remove { get; set; }
        public string ListMultipleSelected { get; set; }
        public string ListSingleSelected { get; set; }
        public string Check { get; set; }
        public string Filter { get; set; }
        public string Clear { get; set; }
        public string CaretDown { get; set; }
    }

    public struct GriddlyCss
    {
        public string TextCenter { get; set; }
        public string TextRight { get; set; }
        public string FloatRight { get; set; }
        public string GriddlyDefault { get; set; }
        public string TableDefault { get; set; }
        public string ButtonDefault { get; set; }
        public GriddlyCssIcons Icons { get; set; }
        public bool IsBootstrap4 { get; set; }

        public static GriddlyCss Bootstrap3Defaults = new GriddlyCss()
        {
            TextCenter = "text-center",
            TextRight = "text-right",
            FloatRight = "pull-right",
            GriddlyDefault = null,
            TableDefault = "table table-bordered table-hover",
            ButtonDefault = "btn btn-default",
            Icons = new GriddlyCssIcons()
            {
                Calendar = "glyphicon glyphicon-calendar",
                Remove = "glyphicon glyphicon-remove",
                ListMultipleSelected = "glyphicon glyphicon-ok",
                ListSingleSelected = "glyphicon glyphicon-record",
                Check = "glyphicon glyphicon-check",
                Filter = "glyphicon glyphicon-filter",
                Clear = "glyphicon glyphicon-ban-circle",
                CaretDown = "caret"
            }
        };

        public static GriddlyCss Bootstrap4Defaults = new GriddlyCss()
        {
            IsBootstrap4 = true,
            TextCenter = "text-center",
            TextRight = "text-right",
            FloatRight = "float-right",
            GriddlyDefault = null,
            TableDefault = "table table-bordered table-hover",
            ButtonDefault = "btn btn-outline-secondary",
            Icons = new GriddlyCssIcons()
            {
                Calendar = "fa fa-calendar-alt",
                Remove = "fa fa-times",
                ListMultipleSelected = "fa fa-check",
                ListSingleSelected = "fas fa-check-circle",
                Check = "fa fa-check-square",
                Filter = "fa fa-filter",
                Clear = "fa fa-ban",
                CaretDown = "fa fa-caret-down"
            }
        };
    }

}
