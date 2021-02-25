using System;
using System.Collections.Generic;
using System.Linq;
#if NET45
using System.Web.Mvc;
using System.Web.Mvc;
#else
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
#endif

namespace Griddly.Mvc
{
    public static class GriddlyFilterExtensions
    {
        // ********
        // NOTE: these methods can be called with null column by the ones on GriddlySettings
        // ********
        public static GriddlyFilterBox FilterBox(this GriddlyColumn column, FilterDataType dataType = FilterDataType.Decimal, string field = null, string caption = null, string htmlClass = null, string captionPlural = null, string group = null, string captionInline = null, object inputHtmlAttributes = null)
        {
            if (caption == null)
                caption = column.Caption;

            if (field == null)
                field = GetField(column);

            if (string.IsNullOrWhiteSpace(caption))
                throw new ArgumentNullException("caption", "Caption must be specified.");
            if (string.IsNullOrWhiteSpace(field))
                throw new ArgumentNullException("field", "Field must be specified.");

            var filter = new GriddlyFilterBox()
            {
                Field = field,
                Caption = caption,
                DataType = dataType,
                HtmlClass = htmlClass,
                Group = group
            };

            if (inputHtmlAttributes != null)
                filter.InputHtmlAttributes = HtmlHelper.AnonymousObjectToHtmlAttributes(inputHtmlAttributes);

            if (captionPlural != null)
                filter.CaptionPlural = captionPlural;

            if (captionInline != null)
                filter.CaptionInline = captionInline;

            return filter;
        }

        public static GriddlyFilterRange FilterRange(this GriddlyColumn column, FilterDataType dataType = FilterDataType.Decimal, string field = null, string fieldEnd = null, string caption = null, string htmlClass = null, string captionPlural = null, string group = null, string captionInline = null, object inputHtmlAttributes = null)
        {
            if (caption == null)
                caption = column.Caption;

            if (field == null)
                field = GetField(column) + "Start";
            if (fieldEnd == null)
                fieldEnd = GetField(column) + "End";

            if (string.IsNullOrWhiteSpace(caption))
                throw new ArgumentNullException("caption", "Caption must be specified.");
            if (string.IsNullOrWhiteSpace(field))
                throw new ArgumentNullException("field", "Field must be specified.");
            if (string.IsNullOrWhiteSpace(fieldEnd))
                throw new ArgumentNullException("fieldEnd", "End field must be specified.");

            var filter = new GriddlyFilterRange()
            {
                Field = field,
                FieldEnd = fieldEnd,
                Caption = caption,
                DataType = dataType,
                HtmlClass = htmlClass,
                Group = group
            };

            if (inputHtmlAttributes != null)
                filter.InputHtmlAttributes = HtmlHelper.AnonymousObjectToHtmlAttributes(inputHtmlAttributes);

            if (captionPlural != null)
                filter.CaptionPlural = captionPlural;

            if (captionInline != null)
                filter.CaptionInline = captionInline;

            return filter;
        }

        public static GriddlyFilterList FilterList(this GriddlyColumn column, IEnumerable<SelectListItem> items, bool isMultiple = true, bool defaultSelectAll = false, string nullItemText = null, bool isNoneAll = true, string field = null, string caption = null, string htmlClass = null, string captionPlural = null, bool displayIncludeCaption = false, string group = null, string captionInline = null, object inputHtmlAttributes = null)
        {
            if (caption == null)
                caption = column.Caption;

            if (field == null)
                field = GetField(column);

            if (string.IsNullOrWhiteSpace(caption))
                throw new ArgumentNullException("caption", "Caption must be specified.");
            if (string.IsNullOrWhiteSpace(field))
                throw new ArgumentNullException("field", "Field must be specified.");

            List<SelectListItem> itemsList = new List<SelectListItem>();

            if (!string.IsNullOrWhiteSpace(nullItemText))
                itemsList.Add(new SelectListItem() { Text = nullItemText, Value = "" });

            itemsList.AddRange(items);

            List<SelectListItem> selectableItemsList = new List<SelectListItem>();

            selectableItemsList.AddRange(itemsList.Where(x => !(x is SelectListItemGroup)));
            selectableItemsList.AddRange(itemsList.Where(x => x is SelectListItemGroup).SelectMany(x => ((SelectListItemGroup)x).Items));

            var filter = new GriddlyFilterList()
            {
                Field = field,
                Items = itemsList,
                SelectableItems = selectableItemsList,
                Caption = caption,
                IsMultiple = isMultiple,
                IsNoneAll = isNoneAll,
                IsNullable = !string.IsNullOrWhiteSpace(nullItemText),
                DefaultSelectAll = defaultSelectAll,
                HtmlClass = htmlClass,
                DisplayIncludeCaption = displayIncludeCaption,
                Group = group
            };

            if (inputHtmlAttributes != null)
                filter.InputHtmlAttributes = HtmlHelper.AnonymousObjectToHtmlAttributes(inputHtmlAttributes);

            if (captionPlural != null)
                filter.CaptionPlural = captionPlural;

            if (captionInline != null)
                filter.CaptionInline = captionInline;

            return filter;
        }

        public static string GetField(GriddlyColumn column)
        {
            string value = null;

            if (column.ExpressionString != null)
            {
                value = column.ExpressionString.Split('.').Last();

                if (value.Length > 1)
                    value = char.ToLower(value[0]) + value.Substring(1);
            }

            return value;
        }

        public static GriddlyFilterList FilterEnum<T>(this GriddlyColumn column, bool isMultiple = true, bool defaultSelectAll = false, string nullItemText = null, bool isNoneAll = true, string field = null, string caption = null, string htmlClass = null, string captionPlural = null, bool displayIncludeCaption = false, string group = null, string captionInline = null, object inputHtmlAttributes = null)
            where T : struct
        {
            return column.FilterList(Extensions.ToSelectListItems<T>().OrderBy(x => x.Text), isMultiple, defaultSelectAll, nullItemText, isNoneAll, field, caption, htmlClass, captionPlural, displayIncludeCaption, group, captionInline, inputHtmlAttributes);
        }

        public static GriddlyFilterList FilterEnum<T>(this GriddlyColumn column, IEnumerable<T> items, bool isMultiple = true, bool defaultSelectAll = false, string nullItemText = null, bool isNoneAll = true, string field = null, string caption = null, string htmlClass = null, string captionPlural = null, bool displayIncludeCaption = false, string group = null, string captionInline = null, bool sort = true, object inputHtmlAttributes = null)
            where T : struct
        {
            var sl = Extensions.ToSelectListItems(items);
            if (sort) sl = sl.OrderBy(x => x.Text);
            return column.FilterList(sl, isMultiple, defaultSelectAll, nullItemText, isNoneAll, field, caption, htmlClass, captionPlural, displayIncludeCaption, group, captionInline, inputHtmlAttributes);
        }

        public static GriddlyFilterList FilterBool(this GriddlyColumn column, string trueLabel = "Yes", string falseLabel = "No", string nullItemText = null, bool isMultiple = false, bool defaultSelectAll = false, bool isNoneAll = false, string field = null, string caption = null, string htmlClass = null, string captionPlural = null, bool displayIncludeCaption = true, string group = null, string captionInline = null, object inputHtmlAttributes = null)
        {
            return column.FilterList(BuildBoolItems(trueLabel, falseLabel), isMultiple, defaultSelectAll, nullItemText, isNoneAll, field, caption, htmlClass, captionPlural, displayIncludeCaption, group, captionInline, inputHtmlAttributes);
        }

        static List<SelectListItem> BuildBoolItems(string trueLabel, string falseLabel)
        {
            return new List<SelectListItem>()
            {
                new SelectListItem() { Value = "true", Text = trueLabel },
                new SelectListItem() { Value = "false", Text = falseLabel },
            };
        }
    }
}
